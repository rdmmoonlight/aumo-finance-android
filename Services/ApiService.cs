using Npgsql;
using AumoFinance.Models;

namespace AumoFinance.Services;

// Akses LANGSUNG ke Neon (Postgres) lewat connection string. Tidak lewat
// API web sama sekali. Semua tulis (INSERT) hanya menyentuh tabel mobile
// ("MobileJournalEntries" / "MobileJournalEntryLines") — tidak pernah
// menyentuh "JournalEntries"/"JournalEntryLines" milik web secara langsung.
// Data mobile baru masuk pembukuan utama setelah diverifikasi lewat
// halaman web "Mobile Classification".
public class ApiService
{
    // TODO: ganti dengan connection string Neon yang sebenarnya.
    // PENTING: string ini ikut ter-bundle di dalam APK dan bisa diekstrak
    // lewat reverse-engineering. Pakai role Postgres dengan hak akses
    // terbatas (hanya INSERT ke MobileJournalEntries/Lines + SELECT ke
    // ChartOfAccounts/JournalEntries/Periods untuk dashboard), jangan role
    // admin/owner database.
    private const string ConnectionString =
        "Host=ep-wandering-bread-ao1sazxn-pooler.c-2.ap-southeast-1.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_mobile;Password=npg_exhf4N9TaStH;SSL Mode=Require;Trust Server Certificate=true";

    private static NpgsqlConnection CreateConnection() => new(ConnectionString);

    // Dashboard: baca dari tabel utama (JournalEntries) yang SUDAH
    // terverifikasi. Entri mobile yang masih Pending tidak memengaruhi
    // angka ini sama sekali.
    public async Task<DashboardModel?> GetDashboardAsync()
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();

            string activePeriod = "-";
            DateTime? periodStart = null;
            DateTime? periodEnd = null;

            await using (var cmd = new NpgsqlCommand(
                "SELECT \"PeriodName\", \"StartDate\", \"EndDate\" FROM \"Periods\" " +
                "WHERE \"IsClosed\" = FALSE ORDER BY \"StartDate\" DESC LIMIT 1", conn))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    activePeriod = reader.GetString(0);
                    periodStart = reader.GetDateTime(1);
                    periodEnd = reader.GetDateTime(2);
                }
            }

            decimal totalCash = 0;
            await using (var cmd = new NpgsqlCommand(
                "SELECT COALESCE(SUM(l.\"Debit\" - l.\"Credit\"), 0) " +
                "FROM \"ChartOfAccounts\" a " +
                "JOIN \"JournalEntryLines\" l ON l.\"AccountId\" = a.\"Id\" " +
                "WHERE a.\"IsActive\" = TRUE AND a.\"Role\" = 'CashAndEquivalents'", conn))
            {
                totalCash = (decimal)(await cmd.ExecuteScalarAsync() ?? 0m);
            }

            decimal revenue = 0;
            decimal expenses = 0;

            const string periodFilter =
                "AND (@hasPeriod = FALSE OR (e.\"EntryDate\" >= @start AND e.\"EntryDate\" <= @end)) ";

            await using (var cmd = new NpgsqlCommand(
                "SELECT a.\"Type\", " +
                "   CASE WHEN a.\"Type\" IN ('OperatingExpenses','OtherExpenses') " +
                "        THEN COALESCE(SUM(l.\"Debit\" - l.\"Credit\"), 0) " +
                "        ELSE COALESCE(SUM(l.\"Credit\" - l.\"Debit\"), 0) END " +
                "FROM \"ChartOfAccounts\" a " +
                "JOIN \"JournalEntryLines\" l ON l.\"AccountId\" = a.\"Id\" " +
                "JOIN \"JournalEntries\" e ON e.\"Id\" = l.\"JournalEntryId\" " +
                "WHERE a.\"IsActive\" = TRUE AND a.\"Type\" IN ('OperatingIncome','OtherIncome','OperatingExpenses','OtherExpenses') " +
                periodFilter +
                "GROUP BY a.\"Type\"", conn))
            {
                cmd.Parameters.AddWithValue("hasPeriod", periodStart.HasValue);
                cmd.Parameters.AddWithValue("start", (object?)periodStart ?? DBNull.Value);
                cmd.Parameters.AddWithValue("end", (object?)periodEnd ?? DBNull.Value);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var type = reader.GetString(0);
                    var net = reader.GetDecimal(1);
                    if (type is "OperatingIncome" or "OtherIncome") revenue += net;
                    else expenses += net;
                }
            }

            return new DashboardModel
            {
                TotalCash = totalCash,
                Revenue = revenue,
                Expenses = expenses,
                NetIncome = revenue - expenses,
                ActivePeriod = activePeriod
            };
        }
        catch
        {
            return null;
        }
    }

    // Ambil daftar akun aktif
    public async Task<List<AccountLookupModel>> GetAccountsAsync()
    {
        var result = new List<AccountLookupModel>();
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                "SELECT \"Id\", \"AccountName\", \"ReferenceNumber\" FROM \"ChartOfAccounts\" " +
                "WHERE \"IsActive\" = TRUE ORDER BY \"ReferenceNumber\"", conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new AccountLookupModel
                {
                    Id = reader.GetInt32(0),
                    AccountName = reader.GetString(1),
                    ReferenceNumber = reader.GetInt32(2)
                });
            }
        }
        catch
        {
            // dikembalikan kosong; halaman pemanggil menampilkan pesan gagal koneksi
        }

        return result;
    }

    // Jurnal manual (akun dipilih sendiri di app). Ditulis LANGSUNG ke
    // MobileJournalEntries + MobileJournalEntryLines dengan Status =
    // 'Pending'. Tidak pernah menyentuh JournalEntries.
    public async Task<(bool success, string message)> PostJournalAsync(CreateJournalDto dto)
    {
        var lines = dto.Lines.Where(l => l.AccountId != 0 && (l.Debit != 0 || l.Credit != 0)).ToList();
        if (lines.Count < 2)
        {
            return (false, "Jurnal harus memiliki minimal dua baris.");
        }

        var totalDebit = lines.Sum(l => l.Debit);
        var totalCredit = lines.Sum(l => l.Credit);
        if (totalDebit != totalCredit || totalDebit == 0)
        {
            return (false, $"Total Debit (Rp {totalDebit:N0}) dan Kredit (Rp {totalCredit:N0}) harus seimbang.");
        }

        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();

            int mobileEntryId;
            await using (var cmd = new NpgsqlCommand(
                "INSERT INTO \"MobileJournalEntries\" " +
                "(\"EntryDate\", \"Mode\", \"Status\", \"SubmittedAt\") " +
                "VALUES (@entryDate, 'Manual', 'Pending', now() AT TIME ZONE 'utc') " +
                "RETURNING \"Id\"", conn, tx))
            {
                cmd.Parameters.AddWithValue("entryDate", dto.EntryDate.Date);
                mobileEntryId = (int)(await cmd.ExecuteScalarAsync())!;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                await using var cmd = new NpgsqlCommand(
                    "INSERT INTO \"MobileJournalEntryLines\" " +
                    "(\"MobileJournalEntryId\", \"AccountId\", \"LineDescription\", \"Debit\", \"Credit\", \"LineOrder\") " +
                    "VALUES (@entryId, @accountId, @desc, @debit, @credit, @order)", conn, tx);
                cmd.Parameters.AddWithValue("entryId", mobileEntryId);
                cmd.Parameters.AddWithValue("accountId", line.AccountId);
                cmd.Parameters.AddWithValue("desc", (object?)line.LineDescription ?? DBNull.Value);
                cmd.Parameters.AddWithValue("debit", line.Debit);
                cmd.Parameters.AddWithValue("credit", line.Credit);
                cmd.Parameters.AddWithValue("order", i);
                await cmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
            return (true, "Jurnal berhasil dikirim, menunggu verifikasi.");
        }
        catch (Exception ex)
        {
            return (false, $"Error koneksi: {ex.Message}");
        }
    }

    // Transaksi cepat (Pemasukan/Pengeluaran). Ditulis LANGSUNG ke
    // MobileJournalEntries dengan Status = 'Pending'. Tidak pernah
    // menyentuh JournalEntries.
    public async Task<(bool success, string message)> PostSimpleTransactionAsync(CreateSimpleTransactionDto dto)
    {
        if (dto.Amount <= 0)
        {
            return (false, "Nominal harus lebih besar dari 0.");
        }

        if (dto.Type != "Income" && dto.Type != "Expense")
        {
            return (false, "Jenis transaksi tidak valid.");
        }

        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                "INSERT INTO \"MobileJournalEntries\" " +
                "(\"EntryDate\", \"Mode\", \"Type\", \"Amount\", \"Note\", \"Status\", \"SubmittedAt\") " +
                "VALUES (@entryDate, 'Simple', @type, @amount, @note, 'Pending', now() AT TIME ZONE 'utc')", conn);
            cmd.Parameters.AddWithValue("entryDate", dto.EntryDate.Date);
            cmd.Parameters.AddWithValue("type", dto.Type);
            cmd.Parameters.AddWithValue("amount", dto.Amount);
            cmd.Parameters.AddWithValue("note", (object?)dto.Note ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
            return (true, "Transaksi berhasil dikirim, menunggu verifikasi.");
        }
        catch (Exception ex)
        {
            return (false, $"Error koneksi: {ex.Message}");
        }
    }
}

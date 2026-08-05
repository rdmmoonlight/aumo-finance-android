using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using AumoFinance.Models;

namespace AumoFinance.Services;

public class ApiService
{
    public const string ConnectionString = "__NEON_CONNECTION_STRING__";

    private readonly NpgsqlDataSource _dataSource = NpgsqlDataSource.Create(ConnectionString);

    private NpgsqlConnection CreateConnection() => _dataSource.CreateConnection();

    private static string DescribeException(Exception ex) => ex switch
    {
        NpgsqlException { InnerException: TimeoutException } => "Koneksi ke server database timeout.",
        NpgsqlException npgEx when npgEx.IsTransient => "Server database sedang tidak dapat dijangkau, coba lagi.",
        Npgsql.PostgresException pgEx => $"Database menolak permintaan: {pgEx.MessageText}",
        TimeoutException => "Koneksi ke server database timeout.",
        _ => $"Terjadi kesalahan: {ex.Message}"
    };

    public async Task<(bool success, string message, string? userId)> LoginAsync(string usernameOrEmail, string password)
    {
        if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Username/Email dan password harus diisi.", null);
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await using var conn = CreateConnection();
            await conn.OpenAsync(cts.Token);

            string normalizedInput = usernameOrEmail.ToUpperInvariant();

            // Query langsung ke tabel AspNetUsers bawaan ASP.NET Core Identity di Neon DB
            await using var cmd = new NpgsqlCommand(
                "SELECT \"Id\", \"PasswordHash\" FROM \"AspNetUsers\" " +
                "WHERE \"NormalizedUserName\" = @input OR \"NormalizedEmail\" = @input LIMIT 1", conn);

            cmd.CommandTimeout = 15;
            cmd.Parameters.AddWithValue("input", normalizedInput);

            await using var reader = await cmd.ExecuteReaderAsync(cts.Token);
            if (await reader.ReadAsync())
            {
                string userId = reader.GetString(0);
                string? hashedPassword = reader.IsDBNull(1) ? null : reader.GetString(1);

                if (string.IsNullOrEmpty(hashedPassword))
                {
                    return (false, "Akun tidak memiliki password yang terkonfigurasi.", null);
                }

                bool isPasswordValid = VerifyIdentityPasswordHash(password, hashedPassword);

                if (isPasswordValid)
                {
                    return (true, "Login berhasil.", userId);
                }
            }

            return (false, "Email/Username atau password salah.", null);
        }
        catch (OperationCanceledException)
        {
            return (false, "Koneksi ke database timeout. Periksa koneksi internet Anda.", null);
        }
        catch (Exception ex)
        {
            return (false, DescribeException(ex), null);
        }
    }

    private static bool VerifyIdentityPasswordHash(string password, string hashedPassword)
    {
        try
        {
            byte[] decodedHashedPassword = Convert.FromBase64String(hashedPassword);

            if (decodedHashedPassword.Length < 1 || decodedHashedPassword[0] != 0x01)
            {
                return false;
            }

            var prf = (Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivationPrf)ReadNetworkByteOrder(decodedHashedPassword, 1);
            int iterCount = (int)ReadNetworkByteOrder(decodedHashedPassword, 5);
            int saltLength = (int)ReadNetworkByteOrder(decodedHashedPassword, 9);

            if (saltLength < 128 / 8)
            {
                return false;
            }

            byte[] salt = new byte[saltLength];
            Buffer.BlockCopy(decodedHashedPassword, 13, salt, 0, salt.Length);

            int subkeyLength = decodedHashedPassword.Length - 13 - salt.Length;
            if (subkeyLength < 128 / 8)
            {
                return false;
            }

            byte[] expectedSubkey = new byte[subkeyLength];
            Buffer.BlockCopy(decodedHashedPassword, 13 + salt.Length, expectedSubkey, 0, expectedSubkey.Length);

            byte[] actualSubkey = Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: prf,
                iterationCount: iterCount,
                numBytesRequested: subkeyLength);

            return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
        }
        catch
        {
            return false;
        }
    }

    private static uint ReadNetworkByteOrder(byte[] buffer, int offset)
    {
        return ((uint)buffer[offset] << 24)
            | ((uint)buffer[offset + 1] << 16)
            | ((uint)buffer[offset + 2] << 8)
            | buffer[offset + 3];
    }

    public async Task<DashboardModel?> GetDashboardAsync()
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();

            string activePeriod = "-";
            DateTime? periodStart = null;
            DateTime? periodEnd = null;
            bool isClosed = false;

            await using (var cmd = new NpgsqlCommand(
                "SELECT \"PeriodName\", \"StartDate\", \"EndDate\", \"IsClosed\" FROM \"Periods\" " +
                "WHERE \"IsClosed\" = FALSE ORDER BY \"StartDate\" DESC LIMIT 1", conn))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    activePeriod = reader.GetString(0);
                    periodStart = reader.GetDateTime(1);
                    periodEnd = reader.GetDateTime(2);
                    isClosed = reader.GetBoolean(3);
                }
                else
                {
                    isClosed = true;
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
                ActivePeriod = activePeriod,
                IsClosed = isClosed
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetDashboardAsync gagal: {DescribeException(ex)}");
            return null;
        }
    }

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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetAccountsAsync gagal: {DescribeException(ex)}");
        }

        return result;
    }

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
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await using var conn = CreateConnection();
            await conn.OpenAsync(cts.Token);
            await using var tx = await conn.BeginTransactionAsync(cts.Token);

            DateTime entryDateUtc = new DateTime(dto.EntryDate.Year, dto.EntryDate.Month, dto.EntryDate.Day, 0, 0, 0, DateTimeKind.Utc);

            int mobileEntryId;
            await using (var cmd = new NpgsqlCommand(
                "INSERT INTO \"MobileJournalEntries\" " +
                "(\"EntryDate\", \"Mode\", \"Status\", \"SubmittedAt\") " +
                "VALUES (@entryDate, 'Manual', 'Pending', now() AT TIME ZONE 'utc') " +
                "RETURNING \"Id\"", conn, tx))
            {
                cmd.CommandTimeout = 15;
                var dateParam = cmd.Parameters.Add("entryDate", NpgsqlTypes.NpgsqlDbType.TimestampTz);
                dateParam.Value = entryDateUtc;

                var scalarResult = await cmd.ExecuteScalarAsync(cts.Token);
                mobileEntryId = Convert.ToInt32(scalarResult);
            }

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                await using var cmd = new NpgsqlCommand(
                    "INSERT INTO \"MobileJournalEntryLines\" " +
                    "(\"MobileJournalEntryId\", \"AccountId\", \"LineDescription\", \"Debit\", \"Credit\", \"LineOrder\") " +
                    "VALUES (@entryId, @accountId, @desc, @debit, @credit, @order)", conn, tx);
                cmd.CommandTimeout = 15;
                cmd.Parameters.AddWithValue("entryId", mobileEntryId);
                cmd.Parameters.AddWithValue("accountId", line.AccountId);
                cmd.Parameters.AddWithValue("desc", (object?)line.LineDescription ?? DBNull.Value);
                cmd.Parameters.AddWithValue("debit", line.Debit);
                cmd.Parameters.AddWithValue("credit", line.Credit);
                cmd.Parameters.AddWithValue("order", i);
                await cmd.ExecuteNonQueryAsync(cts.Token);
            }

            await tx.CommitAsync(cts.Token);
            return (true, "Jurnal berhasil dikirim, menunggu verifikasi.");
        }
        catch (OperationCanceledException)
        {
            return (false, "Koneksi ke database timeout (15 detik). Cek jaringan internet Anda.");
        }
        catch (Npgsql.PostgresException pgEx)
        {
            return (false, $"Postgres Error: {pgEx.MessageText}");
        }
        catch (Exception ex)
        {
            return (false, DescribeException(ex));
        }
    }

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
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await using var conn = CreateConnection();
            await conn.OpenAsync(cts.Token);

            DateTime entryDateUtc = new DateTime(dto.EntryDate.Year, dto.EntryDate.Month, dto.EntryDate.Day, 0, 0, 0, DateTimeKind.Utc);

            await using var cmd = new NpgsqlCommand(
                "INSERT INTO \"MobileJournalEntries\" " +
                "(\"EntryDate\", \"Mode\", \"Type\", \"Amount\", \"Note\", \"Status\", \"SubmittedAt\") " +
                "VALUES (@entryDate, 'Simple', @type, @amount, @note, 'Pending', now() AT TIME ZONE 'utc') " +
                "RETURNING \"Id\"", conn);

            cmd.CommandTimeout = 15;

            var dateParam = cmd.Parameters.Add("entryDate", NpgsqlTypes.NpgsqlDbType.TimestampTz);
            dateParam.Value = entryDateUtc;

            cmd.Parameters.AddWithValue("type", dto.Type);
            cmd.Parameters.AddWithValue("amount", dto.Amount);
            cmd.Parameters.AddWithValue("note", (object?)dto.Note ?? DBNull.Value);

            var scalarResult = await cmd.ExecuteScalarAsync(cts.Token);
            if (scalarResult != null && scalarResult != DBNull.Value)
            {
                return (true, "Transaksi berhasil dikirim, menunggu verifikasi.");
            }

            return (false, "Gagal menyimpan: Database tidak mengembalikan ID transaksi.");
        }
        catch (OperationCanceledException)
        {
            return (false, "Koneksi ke database timeout (15 detik). Cek jaringan internet Anda.");
        }
        catch (Npgsql.PostgresException pgEx)
        {
            return (false, $"Postgres Error: {pgEx.MessageText}");
        }
        catch (Exception ex)
        {
            return (false, DescribeException(ex));
        }
    }
}

namespace AumoFinance.Api.Models;

public enum JournalType { General, Adjusting, Closing }

public class JournalLine
{
    public int Id { get; set; }
    public int JournalEntryId { get; set; }
    public JournalEntry JournalEntry { get; set; } = null!;
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

public class JournalEntry
{
    public int Id { get; set; }
    public string TransactionNo { get; set; } = string.Empty;
    // Setiap entri WAJIB terikat ke satu Period — tanpa ini laporan (Trial Balance,
    // Ledger, dst.) tidak bisa difilter per periode. Ini sebelumnya hilang total
    // di model (celah, bukan disengaja) sampai fase 7.2 ini.
    public int PeriodId { get; set; }
    public Period Period { get; set; } = null!;
    // Tanggal manual (date-only) dari date picker pengguna.
    public DateTime EntryDate { get; set; }
    // Waktu lokal perangkat saat input; disimpan sebagai Unspecified agar tidak
    // ikut dikonversi zona waktu oleh server (lihat riwayat bug tanggal mundur 1 hari).
    public DateTime CreatedAt { get; set; }
    public JournalType Type { get; set; }
    public List<JournalLine> Lines { get; set; } = new();
    public bool IsBalanced => Lines.Sum(l => l.Debit) == Lines.Sum(l => l.Credit);
}

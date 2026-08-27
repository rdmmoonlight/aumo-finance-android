namespace AumoFinance.Api.Models;

public enum JournalType { General, Adjusting, Closing }

public class JournalLine
{
    public int AccountId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

public class JournalEntry
{
    public int Id { get; set; }
    public string TransactionNo { get; set; } = string.Empty;
    // Tanggal manual (date-only) dari date picker pengguna.
    public DateTime EntryDate { get; set; }
    // Waktu lokal perangkat saat input; disimpan sebagai Unspecified agar tidak
    // ikut dikonversi zona waktu oleh server (lihat riwayat bug tanggal mundur 1 hari).
    public DateTime CreatedAt { get; set; }
    public JournalType Type { get; set; }
    public List<JournalLine> Lines { get; set; } = new();
    public bool IsBalanced => Lines.Sum(l => l.Debit) == Lines.Sum(l => l.Credit);
}

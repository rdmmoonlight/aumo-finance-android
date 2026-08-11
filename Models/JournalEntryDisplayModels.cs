using System.Globalization;

namespace AumoFinance.Models;

public class JournalEntryDisplayModel
{
    public int Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public List<JournalEntryLineDisplayModel> Lines { get; set; } = new();
}

public class JournalEntryLineDisplayModel
{
    public string AccountName { get; set; } = string.Empty;
    public string RefNumber { get; set; } = string.Empty;
    public string? LineDescription { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }

    public bool IsCredit => Credit > 0;
    public bool HasDescription => !string.IsNullOrWhiteSpace(LineDescription);
    public string AccountTextColor => IsCredit ? "#F87171" : "#4ADE80";
    public string AmountColor => IsCredit ? "#F87171" : "#4ADE80";
    public string FormattedAmount
    {
        get
        {
            var culture = new CultureInfo("id-ID");
            var amount = IsCredit ? Credit : Debit;
            var prefix = IsCredit ? "Cr" : "Dr";
            return $"{prefix}: Rp {amount.ToString("N0", culture)}";
        }
    }
}

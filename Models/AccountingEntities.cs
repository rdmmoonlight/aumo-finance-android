namespace AumoFinance.Models;

public class Period : AccountingPeriod
{
    public Guid UserId { get; set; }
}

public class ChartOfAccount
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class JournalEntry
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string JournalType { get; set; } = "General";
    public List<JournalEntryLine> Lines { get; set; } = new();
}

public class JournalEntryLine
{
    public Guid Id { get; set; }
    public Guid JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }
    public Guid AccountId { get; set; }
    public ChartOfAccount? Account { get; set; }
    public string LineDescription { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public int LineOrder { get; set; }
}

public class SelectedPeriod
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PeriodId { get; set; }
    public Period? Period { get; set; }
}

namespace AumoFinance.Models;

public class CreateJournalDto
{
    public DateTime EntryDate { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public List<JournalLineDto> Lines { get; set; } = new();
}

public class JournalLineDto
{
    public int AccountId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

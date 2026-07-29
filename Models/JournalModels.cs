using System.Text.Json.Serialization;

namespace AumoFinance.Models;

public class AccountLookupModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("accountName")]
    public string AccountName { get; set; } = string.Empty;

    [JsonPropertyName("referenceNumber")]
    public string ReferenceNumber { get; set; } = string.Empty;

    public string DisplayText => $"[{ReferenceNumber}] {AccountName}";
}

public class CreateJournalDto
{
    [JsonPropertyName("journalType")]
    public string JournalType { get; set; } = "General";

    [JsonPropertyName("entryDate")]
    public DateTime EntryDate { get; set; } = DateTime.Today;

    [JsonPropertyName("lines")]
    public List<CreateJournalLineDto> Lines { get; set; } = new();
}

public class CreateJournalLineDto
{
    [JsonPropertyName("accountId")]
    public Guid AccountId { get; set; }

    [JsonPropertyName("lineDescription")]
    public string? LineDescription { get; set; }

    [JsonPropertyName("debit")]
    public decimal Debit { get; set; }

    [JsonPropertyName("credit")]
    public decimal Credit { get; set; }
}

using System.Text.Json.Serialization;

namespace AumoFinance.Models;

public class AccountLookupModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("accountName")]
    public string AccountName { get; set; } = string.Empty;

    [JsonPropertyName("referenceNumber")]
    public int ReferenceNumber { get; set; }

    public string DisplayText => $"[{ReferenceNumber}] {AccountName}";
}

public class CreateJournalDto
{
    [JsonPropertyName("entryDate")]
    public DateTime EntryDate { get; set; } = DateTime.Today;

    [JsonPropertyName("lines")]
    public List<CreateJournalLineDto> Lines { get; set; } = new();
}

public class CreateJournalLineDto
{
    [JsonPropertyName("accountId")]
    public int AccountId { get; set; }

    [JsonPropertyName("lineDescription")]
    public string? LineDescription { get; set; }

    [JsonPropertyName("debit")]
    public decimal Debit { get; set; }

    [JsonPropertyName("credit")]
    public decimal Credit { get; set; }
}

public class CreateSimpleTransactionDto
{
    [JsonPropertyName("entryDate")]
    public DateTime EntryDate { get; set; } = DateTime.Today;

    // "Income" atau "Expense"
    [JsonPropertyName("type")]
    public string Type { get; set; } = "Income";

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }
}

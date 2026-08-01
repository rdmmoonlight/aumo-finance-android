using System.Text.Json.Serialization;

namespace AumoFinance.Models;

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

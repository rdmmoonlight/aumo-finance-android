namespace AumoFinance.Models;

public class CreateSimpleTransactionDto
{
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = "Expense"; // "Income" atau "Expense"
}

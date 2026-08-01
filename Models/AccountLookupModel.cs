namespace AumoFinance.Models;

public class AccountLookupModel
{
    public int Id { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
}

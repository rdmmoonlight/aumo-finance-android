namespace AumoFinance.Api.Models;

public enum AccountType { Permanent, Temporary }

public class Account
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal Balance { get; set; }
}

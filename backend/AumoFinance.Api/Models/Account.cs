namespace AumoFinance.Api.Models;

public enum AccountType { Permanent, Temporary }

// Klasifikasi rinci, dibutuhkan agar Income Statement (Revenue vs Expense) dan
// Financial Position (Asset vs Liability vs Equity) bisa dihitung dengan benar.
// Type (Permanent/Temporary) saja tidak cukup detail untuk itu.
// Asset/Expense = normal saldo Debit. Liability/Equity/Revenue = normal saldo Kredit.
public enum AccountCategory { Asset, Liability, Equity, Revenue, Expense }

public class Account
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public AccountCategory Category { get; set; }
    public bool IsActive { get; set; } = true;
    // Saldo TIDAK disimpan di sini secara langsung — dihitung on-the-fly dari
    // JournalLine (lihat controller Ledger/TrialBalance/dst.), supaya tidak ada
    // risiko field ini basi/tidak sinkron dengan jurnal yang sesungguhnya.
    public List<JournalLine> Lines { get; set; } = new();

    public bool IsNormalDebit => Category is AccountCategory.Asset or AccountCategory.Expense;
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AumoFinance.Models;
using Microsoft.EntityFrameworkCore;

namespace AumoFinance.Services;

public class AccountingService
{
    private readonly AppDbContext _dbContext;

    public AccountingService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // 1. Ambil Periode Aktif/Terakhir
    public async Task<Period?> GetCurrentPeriodAsync(Guid userId)
    {
        return await _dbContext.Periods
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.EndDate)
            .FirstOrDefaultAsync();
    }

    // 2. Ambil Data General Journal Berdasarkan Periode
    public async Task<List<JournalEntry>> GetGeneralJournalAsync(Guid userId, Period period)
    {
        return await _dbContext.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Where(j => j.UserId == userId 
                     && j.EntryDate >= period.StartDate 
                     && j.EntryDate <= period.EndDate)
            .OrderBy(j => j.EntryDate)
            .ThenBy(j => j.Id)
            .ToListAsync();
    }

    // 3. Build General Ledger (Permanen atau Sementara)
    public async Task<List<LedgerAccountViewModel>> GetGeneralLedgerAsync(Guid userId, Period period, bool isTemporary)
    {
        // Filter tipe akun
        var accounts = await _dbContext.ChartOfAccounts
            .Where(a => a.IsActive && a.UserId == userId)
            .OrderBy(a => a.ReferenceNumber)
            .ToListAsync();

        var filteredAccounts = accounts
            .Where(a => isTemporary ? AccountClassification.IsTemporary(a.Type) : AccountClassification.IsPermanent(a.Type))
            .ToList();

        var accountIds = filteredAccounts.Select(a => a.Id).ToList();

        var lines = await _dbContext.JournalEntryLines
            .Include(l => l.JournalEntry)
            .Where(l => accountIds.Contains(l.AccountId) && l.JournalEntry!.UserId == userId)
            .OrderBy(l => l.JournalEntry!.EntryDate)
            .ThenBy(l => l.JournalEntry!.Id)
            .ThenBy(l => l.LineOrder)
            .ToListAsync();

        var result = new List<LedgerAccountViewModel>();

        foreach (var account in filteredAccounts)
        {
            var isPermanent = AccountClassification.IsPermanent(account.Type);
            var normalDebit = AccountClassification.NormalBalanceIsDebit(account.Type);
            decimal running = 0;

            var accountLines = isPermanent
                ? lines.Where(l => l.AccountId == account.Id && l.JournalEntry!.EntryDate <= period.EndDate)
                : lines.Where(l => l.AccountId == account.Id && l.JournalEntry!.EntryDate >= period.StartDate && l.JournalEntry!.EntryDate <= period.EndDate);

            var ledgerLines = new List<LedgerLineViewModel>();
            foreach (var line in accountLines)
            {
                running += normalDebit ? (line.Debit - line.Credit) : (line.Credit - line.Debit);
                ledgerLines.Add(new LedgerLineViewModel
                {
                    EntryDate = line.JournalEntry!.EntryDate,
                    Description = line.LineDescription,
                    Debit = line.Debit,
                    Credit = line.Credit,
                    RunningBalance = running
                });
            }

            result.Add(new LedgerAccountViewModel
            {
                AccountId = account.Id,
                ReferenceNumber = account.ReferenceNumber,
                AccountName = account.AccountName,
                Type = account.Type,
                NormalBalanceIsDebit = normalDebit,
                Lines = ledgerLines,
                EndingBalance = running
            });
        }

        return result;
    }
}

#region Helper Class & Models
public static class AccountClassification
{
    public static bool IsTemporary(string type) => 
        type.Equals("OperatingIncome", StringComparison.OrdinalIgnoreCase) ||
        type.Equals("OperatingExpense", StringComparison.OrdinalIgnoreCase) ||
        type.Equals("OtherIncome", StringComparison.OrdinalIgnoreCase) ||
        type.Equals("OtherExpense", StringComparison.OrdinalIgnoreCase) ||
        type.Equals("Revenue", StringComparison.OrdinalIgnoreCase) ||
        type.Equals("Expense", StringComparison.OrdinalIgnoreCase);

    public static bool IsPermanent(string type) => !IsTemporary(type);

    public static bool NormalBalanceIsDebit(string type) =>
        type.Equals("Asset", StringComparison.OrdinalIgnoreCase) ||
        type.Equals("Expense", StringComparison.OrdinalIgnoreCase) ||
        type.Equals("OperatingExpense", StringComparison.OrdinalIgnoreCase) ||
        type.Equals("OtherExpense", StringComparison.OrdinalIgnoreCase);
}

public class LedgerAccountViewModel
{
    public Guid AccountId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool NormalBalanceIsDebit { get; set; }
    public decimal EndingBalance { get; set; }
    public List<LedgerLineViewModel> Lines { get; set; } = new();

    public string EndingBalanceColor => EndingBalance >= 0 ? "#4ADE80" : "#F87171";
    public string FormattedEndingBalance => $"Saldo: Rp {EndingBalance:N0} ({(NormalBalanceIsDebit ? "Dr" : "Cr")})";
}

public class LedgerLineViewModel
{
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }

    public bool HasDebit => Debit > 0;
    public bool HasCredit => Credit > 0;
    public string FormattedDebit => $"Dr: Rp {Debit:N0}";
    public string FormattedCredit => $"Cr: Rp {Credit:N0}";
    public string FormattedRunningBalance => $"Rp {RunningBalance:N0}";
}
#endregion

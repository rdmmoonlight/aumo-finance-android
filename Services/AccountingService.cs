using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AumoFinance;
using Microsoft.EntityFrameworkCore;
using AumoFinance.Models;

namespace AumoFinance.Services;

public class AccountingService
{
    private readonly AppDbContext _dbContext;

    public AppDbContext DbContext => _dbContext;

    public AccountingService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Period?> GetCurrentPeriodAsync(Guid currentUserId)
    {
        // Mengambil periode aktif berdasarkan user
        return await _dbContext.Periods
            .Where(p => p.UserId == currentUserId)
            .OrderByDescending(p => p.EndDate)
            .FirstOrDefaultAsync();
    }

    public async Task<Period?> GetPeriodByIdAsync(int periodId)
    {
        return await _dbContext.Periods.FindAsync(periodId);
    }

    public async Task<List<JournalEntry>> GetGeneralJournalAsync(Guid currentUserId, Period period)
    {
        return await _dbContext.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Where(j => j.UserId == currentUserId
                     && j.EntryDate >= period.StartDate
                     && j.EntryDate <= period.EndDate)
            .OrderByDescending(j => j.EntryDate)
            .ThenByDescending(j => j.Id)
            .ToListAsync();
    }

    public async Task<List<LedgerAccountViewModel>> GetGeneralLedgerAsync(Guid currentUserId, Period period, bool isTemporary)
    {
        var accounts = await _dbContext.ChartOfAccounts
            .Where(a => a.IsActive && a.UserId == currentUserId)
            .OrderBy(a => a.ReferenceNumber)
            .ToListAsync();

        var filteredAccounts = accounts
            .Where(a => isTemporary ? AccountClassification.IsTemporary(a.Type) : AccountClassification.IsPermanent(a.Type))
            .ToList();

        var accountIds = filteredAccounts.Select(a => a.Id).ToList();

        var lines = await _dbContext.JournalEntryLines
            .Include(l => l.JournalEntry)
            .Where(l => accountIds.Contains(l.AccountId) && l.JournalEntry!.UserId == currentUserId)
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

    public async Task<List<TrialBalanceRowViewModel>> GetTrialBalanceAsync(Guid userId, Period period, bool includeAdjusting)
    {
        var accounts = await _dbContext.ChartOfAccounts
            .Where(a => a.IsActive && a.UserId == userId)
            .OrderBy(a => a.ReferenceNumber)
            .ToListAsync();

        var accountIds = accounts.Select(a => a.Id).ToList();

        var linesQuery = _dbContext.JournalEntryLines
            .Include(l => l.JournalEntry)
            .Where(l => accountIds.Contains(l.AccountId) && l.JournalEntry!.UserId == userId);

        var lines = includeAdjusting
            ? await linesQuery.Where(l => l.JournalEntry!.JournalType == "General" || l.JournalEntry!.JournalType == "Adjusting").ToListAsync()
            : await linesQuery.Where(l => l.JournalEntry!.JournalType == "General").ToListAsync();

        var rows = new List<TrialBalanceRowViewModel>();

        foreach (var account in accounts)
        {
            var isPermanent = AccountClassification.IsPermanent(account.Type);
            var normalDebit = AccountClassification.NormalBalanceIsDebit(account.Type);

            var accountLines = isPermanent
                ? lines.Where(l => l.AccountId == account.Id && l.JournalEntry!.EntryDate <= period.EndDate).ToList()
                : lines.Where(l => l.AccountId == account.Id && l.JournalEntry!.EntryDate >= period.StartDate && l.JournalEntry!.EntryDate <= period.EndDate).ToList();

            var netBalance = normalDebit
                ? accountLines.Sum(l => l.Debit - l.Credit)
                : accountLines.Sum(l => l.Credit - l.Debit);

            if (!accountLines.Any() && netBalance == 0) continue;

            rows.Add(new TrialBalanceRowViewModel
            {
                AccountId = account.Id,
                ReferenceNumber = account.ReferenceNumber,
                AccountName = account.AccountName,
                Type = account.Type,
                Role = account.Role,
                NormalBalanceIsDebit = normalDebit,
                NetBalance = netBalance
            });
        }

        return rows;
    }
}

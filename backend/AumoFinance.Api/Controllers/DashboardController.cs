using AumoFinance.Api.Data;
using AumoFinance.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AumoFinance.Api.Controllers;

public record DashboardSummary(decimal TotalAssets, decimal TotalLiabilities, decimal TotalEquity, decimal NetIncome);

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    // Ringkasan cepat: Total Aset/Liabilitas/Ekuitas mengikuti aturan Financial
    // Position (kumulatif sejak awal), Laba Bersih mengikuti aturan Income
    // Statement (periode berjalan saja, General+Adjusting, tanpa Closing).
    [HttpGet]
    public async Task<IActionResult> GetSummary([FromQuery] int periodId)
    {
        var period = await _db.Periods.FirstOrDefaultAsync(p => p.Id == periodId);
        if (period == null) return Ok(new DashboardSummary(0m, 0m, 0m, 0m));

        var eligiblePeriodIds = await _db.Periods
            .Where(p => p.StartDate <= period.StartDate)
            .Select(p => p.Id)
            .ToListAsync();

        var totalAssets = await _db.JournalLines
            .Where(l => eligiblePeriodIds.Contains(l.JournalEntry.PeriodId) && l.Account.Category == AccountCategory.Asset)
            .SumAsync(l => (decimal?)(l.Debit - l.Credit)) ?? 0m;

        var totalLiabilities = await _db.JournalLines
            .Where(l => eligiblePeriodIds.Contains(l.JournalEntry.PeriodId) && l.Account.Category == AccountCategory.Liability)
            .SumAsync(l => (decimal?)(l.Credit - l.Debit)) ?? 0m;

        var totalEquity = await _db.JournalLines
            .Where(l => eligiblePeriodIds.Contains(l.JournalEntry.PeriodId) && l.Account.Category == AccountCategory.Equity)
            .SumAsync(l => (decimal?)(l.Credit - l.Debit)) ?? 0m;

        var revenue = await _db.JournalLines
            .Where(l => l.JournalEntry.PeriodId == periodId && l.JournalEntry.Type != JournalType.Closing && l.Account.Category == AccountCategory.Revenue)
            .SumAsync(l => (decimal?)(l.Credit - l.Debit)) ?? 0m;

        var expense = await _db.JournalLines
            .Where(l => l.JournalEntry.PeriodId == periodId && l.JournalEntry.Type != JournalType.Closing && l.Account.Category == AccountCategory.Expense)
            .SumAsync(l => (decimal?)(l.Debit - l.Credit)) ?? 0m;

        return Ok(new DashboardSummary(totalAssets, totalLiabilities, totalEquity, revenue - expense));
    }
}

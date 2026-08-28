using AumoFinance.Api.Data;
using AumoFinance.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AumoFinance.Api.Controllers;

public record AccountAmountDto(string AccountName, decimal Amount);

public record IncomeStatementReportDto(
    List<AccountAmountDto> RevenueAccounts, decimal TotalRevenue,
    List<AccountAmountDto> ExpenseAccounts, decimal TotalExpenses,
    decimal OperatingIncome,
    List<AccountAmountDto> OtherIncomeAccounts,
    List<AccountAmountDto> OtherExpenseAccounts,
    decimal NetIncome
);

[ApiController]
[Route("api/incomestatement")]
[Authorize]
public class IncomeStatementController : ControllerBase
{
    private readonly AppDbContext _db;

    public IncomeStatementController(AppDbContext db)
    {
        _db = db;
    }

    // Dihitung dari akun Category=Revenue/Expense, jurnal General+Adjusting saja
    // (Closing tidak dihitung — itu justru menutup akun Temporary ini ke Retained
    // Earnings, bukan bagian dari laporan). "Other Income/Expense" belum punya
    // pembeda tersendiri di skema (butuh sub-kategori operating vs non-operating
    // yang belum ada) — untuk saat ini selalu kosong, jadi Operating Income = Net Income
    // sampai skema diperluas.
    [HttpGet]
    public async Task<IActionResult> GetIncomeStatement([FromQuery] int periodId)
    {
        var revenueLines = await _db.JournalLines
            .Where(l => l.JournalEntry.PeriodId == periodId
                && l.JournalEntry.Type != JournalType.Closing
                && l.Account.Category == AccountCategory.Revenue)
            .GroupBy(l => l.Account.Name)
            .Select(g => new AccountAmountDto(g.Key, g.Sum(l => l.Credit) - g.Sum(l => l.Debit)))
            .ToListAsync();

        var expenseLines = await _db.JournalLines
            .Where(l => l.JournalEntry.PeriodId == periodId
                && l.JournalEntry.Type != JournalType.Closing
                && l.Account.Category == AccountCategory.Expense)
            .GroupBy(l => l.Account.Name)
            .Select(g => new AccountAmountDto(g.Key, g.Sum(l => l.Debit) - g.Sum(l => l.Credit)))
            .ToListAsync();

        var totalRevenue = revenueLines.Sum(r => r.Amount);
        var totalExpenses = expenseLines.Sum(e => e.Amount);
        var operatingIncome = totalRevenue - totalExpenses;

        var report = new IncomeStatementReportDto(
            revenueLines, totalRevenue,
            expenseLines, totalExpenses,
            operatingIncome,
            new List<AccountAmountDto>(),
            new List<AccountAmountDto>(),
            operatingIncome);

        return Ok(report);
    }
}

using AumoFinance.Api.Data;
using AumoFinance.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AumoFinance.Api.Controllers;

public record CashFlowReportDto(
    List<AccountAmountDto> OperatingActivities, decimal NetOperating,
    List<AccountAmountDto> InvestingActivities, decimal NetInvesting,
    List<AccountAmountDto> FinancingActivities, decimal NetFinancing,
    decimal NetChangeInCash, decimal EndingCashBalance
);

[ApiController]
[Route("api/cashflow")]
[Authorize]
public class CashFlowController : ControllerBase
{
    private readonly AppDbContext _db;

    public CashFlowController(AppDbContext db)
    {
        _db = db;
    }

    // Skema saat ini belum punya klasifikasi Operating/Investing/Financing per akun
    // (itu butuh field baru, sama seperti Category kemarin). Sebagai pendekatan
    // sementara yang tetap akurat secara total: seluruh mutasi akun Kas/Bank
    // (dicari lewat nama akun mengandung "Kas"/"Bank"/"Cash") dilaporkan sebagai
    // Operating, dan EndingCashBalance dihitung dari saldo kumulatif akun tsb
    // (konsisten dengan Financial Position: kumulatif sejak awal, bukan per periode).
    // TODO: tambah field AccountCashFlowCategory eksplisit di Account untuk
    // pemisahan Operating/Investing/Financing yang benar per akun.
    [HttpGet]
    public async Task<IActionResult> GetCashFlow([FromQuery] int periodId)
    {
        var period = await _db.Periods.FirstOrDefaultAsync(p => p.Id == periodId);
        if (period == null) return NotFound(new { message = "Periode tidak ditemukan." });

        var cashMovements = await _db.JournalLines
            .Where(l => l.JournalEntry.PeriodId == periodId
                && l.Account.Category == AccountCategory.Asset
                && (l.Account.Name.Contains("Kas") || l.Account.Name.Contains("Bank") || l.Account.Name.Contains("Cash")))
            .GroupBy(l => l.Account.Name)
            .Select(g => new AccountAmountDto(g.Key, g.Sum(l => l.Debit) - g.Sum(l => l.Credit)))
            .ToListAsync();

        var netOperating = cashMovements.Sum(c => c.Amount);

        var eligiblePeriodIds = await _db.Periods
            .Where(p => p.StartDate <= period.StartDate)
            .Select(p => p.Id)
            .ToListAsync();

        var endingCashBalance = await _db.JournalLines
            .Where(l => eligiblePeriodIds.Contains(l.JournalEntry.PeriodId)
                && l.Account.Category == AccountCategory.Asset
                && (l.Account.Name.Contains("Kas") || l.Account.Name.Contains("Bank") || l.Account.Name.Contains("Cash")))
            .SumAsync(l => (decimal?)(l.Debit - l.Credit)) ?? 0m;

        var report = new CashFlowReportDto(
            cashMovements, netOperating,
            new List<AccountAmountDto>(), 0m,
            new List<AccountAmountDto>(), 0m,
            netOperating, endingCashBalance);

        return Ok(report);
    }
}

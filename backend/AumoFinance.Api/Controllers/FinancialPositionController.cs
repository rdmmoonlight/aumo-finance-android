using AumoFinance.Api.Data;
using AumoFinance.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AumoFinance.Api.Controllers;

public record FinancialPositionReportDto(
    List<AccountAmountDto> AssetAccounts, decimal TotalAssets,
    List<AccountAmountDto> LiabilityAccounts, decimal TotalLiabilities,
    List<AccountAmountDto> EquityAccounts, decimal TotalEquity
);

[ApiController]
[Route("api/financialposition")]
[Authorize]
public class FinancialPositionController : ControllerBase
{
    private readonly AppDbContext _db;

    public FinancialPositionController(AppDbContext db)
    {
        _db = db;
    }

    // Neraca (Financial Position): akun Permanent bersifat KUMULATIF sejak awal
    // berdirinya usaha, bukan hanya periode berjalan — berbeda dari General Ledger
    // yang dibatasi ketat per periode. Jadi di sini kita jumlahkan seluruh
    // JournalLine pada akun Permanent dari SEMUA periode hingga dan termasuk
    // periode yang dipilih (PeriodId <= periodId berdasarkan urutan StartDate),
    // bukan hanya PeriodId == periodId.
    // TotalAssets harus sama dengan TotalLiabilities + TotalEquity.
    [HttpGet]
    public async Task<IActionResult> GetFinancialPosition([FromQuery] int periodId)
    {
        var period = await _db.Periods.FirstOrDefaultAsync(p => p.Id == periodId);
        if (period == null) return NotFound(new { message = "Periode tidak ditemukan." });

        var eligiblePeriodIds = await _db.Periods
            .Where(p => p.StartDate <= period.StartDate)
            .Select(p => p.Id)
            .ToListAsync();

        var assetAccounts = await BalancesFor(AccountCategory.Asset, eligiblePeriodIds, debitPositive: true);
        var liabilityAccounts = await BalancesFor(AccountCategory.Liability, eligiblePeriodIds, debitPositive: false);
        var equityAccounts = await BalancesFor(AccountCategory.Equity, eligiblePeriodIds, debitPositive: false);

        var report = new FinancialPositionReportDto(
            assetAccounts, assetAccounts.Sum(a => a.Amount),
            liabilityAccounts, liabilityAccounts.Sum(a => a.Amount),
            equityAccounts, equityAccounts.Sum(a => a.Amount));

        return Ok(report);
    }

    private async Task<List<AccountAmountDto>> BalancesFor(AccountCategory category, List<int> periodIds, bool debitPositive)
    {
        // Closing entries SENGAJA disertakan di sini (tidak difilter keluar) —
        // itu yang memindahkan Laba Bersih dari akun Temporary ke Retained Earnings
        // (Equity). Tanpa Closing, Neraca tidak akan balance setelah periode ditutup.
        var query = _db.JournalLines
            .Where(l => periodIds.Contains(l.JournalEntry.PeriodId)
                && l.Account.Category == category)
            .GroupBy(l => l.Account.Name);

        return debitPositive
            ? await query.Select(g => new AccountAmountDto(g.Key, g.Sum(l => l.Debit) - g.Sum(l => l.Credit))).ToListAsync()
            : await query.Select(g => new AccountAmountDto(g.Key, g.Sum(l => l.Credit) - g.Sum(l => l.Debit))).ToListAsync();
    }
}

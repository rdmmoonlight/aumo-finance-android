using AumoFinance.Api.Data;
using AumoFinance.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AumoFinance.Api.Controllers;

public record RetainedEarningsReportDto(decimal BeginningBalance, decimal NetIncome, decimal Drawings, decimal EndingBalance);

[ApiController]
[Route("api/retainedearnings")]
[Authorize]
public class RetainedEarningsController : ControllerBase
{
    private readonly AppDbContext _db;

    public RetainedEarningsController(AppDbContext db)
    {
        _db = db;
    }

    // Saldo Akhir = Saldo Awal + Laba Bersih - Prive.
    // Saldo Awal: akumulasi Retained Earnings dari SEMUA periode SEBELUM periode
    // ini (StartDate lebih awal) — akun Permanent bersifat kumulatif.
    // Laba Bersih: dihitung ulang persis seperti IncomeStatementController
    // (Revenue - Expense, jurnal General+Adjusting, periode berjalan saja).
    // Prive (Drawings): akun bertipe Equity yang namanya mengandung "Prive"/"Drawing"
    // — belum ada flag eksplisit "IsDrawingsAccount" di skema, jadi untuk saat ini
    // dicocokkan lewat nama akun (TODO: tambah flag eksplisit agar tidak bergantung
    // pada penamaan akun oleh pengguna).
    [HttpGet]
    public async Task<IActionResult> GetRetainedEarnings([FromQuery] int periodId)
    {
        var period = await _db.Periods.FirstOrDefaultAsync(p => p.Id == periodId);
        if (period == null) return NotFound(new { message = "Periode tidak ditemukan." });

        var priorPeriodIds = await _db.Periods
            .Where(p => p.StartDate < period.StartDate)
            .Select(p => p.Id)
            .ToListAsync();

        var beginningBalance = await _db.JournalLines
            .Where(l => priorPeriodIds.Contains(l.JournalEntry.PeriodId)
                && l.Account.Category == AccountCategory.Equity
                && !l.Account.Name.Contains("Prive") && !l.Account.Name.Contains("Drawing"))
            .SumAsync(l => (decimal?)(l.Credit - l.Debit)) ?? 0m;

        var revenueThisPeriod = await _db.JournalLines
            .Where(l => l.JournalEntry.PeriodId == periodId
                && l.JournalEntry.Type != JournalType.Closing
                && l.Account.Category == AccountCategory.Revenue)
            .SumAsync(l => (decimal?)(l.Credit - l.Debit)) ?? 0m;

        var expenseThisPeriod = await _db.JournalLines
            .Where(l => l.JournalEntry.PeriodId == periodId
                && l.JournalEntry.Type != JournalType.Closing
                && l.Account.Category == AccountCategory.Expense)
            .SumAsync(l => (decimal?)(l.Debit - l.Credit)) ?? 0m;

        var netIncome = revenueThisPeriod - expenseThisPeriod;

        var drawings = await _db.JournalLines
            .Where(l => l.JournalEntry.PeriodId == periodId
                && l.Account.Category == AccountCategory.Equity
                && (l.Account.Name.Contains("Prive") || l.Account.Name.Contains("Drawing")))
            .SumAsync(l => (decimal?)(l.Debit - l.Credit)) ?? 0m;

        var report = new RetainedEarningsReportDto(beginningBalance, netIncome, drawings, beginningBalance + netIncome - drawings);
        return Ok(report);
    }
}

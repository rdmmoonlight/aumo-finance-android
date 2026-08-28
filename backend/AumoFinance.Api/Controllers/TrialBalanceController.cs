using AumoFinance.Api.Data;
using AumoFinance.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AumoFinance.Api.Controllers;

public record TrialBalanceRowDto(int AccountId, string AccountName, decimal Debit, decimal Credit);
public record TrialBalanceReportDto(List<TrialBalanceRowDto> Rows, decimal TotalDebit, decimal TotalCredit);

[ApiController]
[Route("api/trialbalance")]
[Authorize]
public class TrialBalanceController : ControllerBase
{
    private readonly AppDbContext _db;

    public TrialBalanceController(AppDbContext db)
    {
        _db = db;
    }

    // ATURAN BISNIS (final, jangan diubah tanpa instruksi eksplisit):
    // - Trial Balance TIDAK disesuaikan (adjusted=false): hanya menghitung jurnal type=General.
    // - Adjusted Trial Balance (adjusted=true): menghitung jurnal type=General + Adjusting.
    // - Jurnal type=Closing TIDAK PERNAH dihitung di kedua varian ini.
    // - Semua akun dibatasi ketat pada rentang tanggal periode yang dipilih
    //   (period.StartDate..EndDate) — TIDAK ada carry-over kumulatif lintas periode.
    [HttpGet]
    public async Task<IActionResult> GetTrialBalance([FromQuery] int periodId, [FromQuery] bool adjusted)
    {
        var period = await _db.Periods.FirstOrDefaultAsync(p => p.Id == periodId);
        if (period == null) return NotFound(new { message = "Periode tidak ditemukan." });

        var allowedTypes = adjusted
            ? new[] { JournalType.General, JournalType.Adjusting }
            : new[] { JournalType.General };

        var rows = await _db.JournalLines
            .Where(l => l.JournalEntry.PeriodId == periodId && allowedTypes.Contains(l.JournalEntry.Type))
            .GroupBy(l => new { l.AccountId, l.Account.Name })
            .Select(g => new TrialBalanceRowDto(
                g.Key.AccountId,
                g.Key.Name,
                g.Sum(l => l.Debit),
                g.Sum(l => l.Credit)))
            .OrderBy(r => r.AccountName)
            .ToListAsync();

        var report = new TrialBalanceReportDto(rows, rows.Sum(r => r.Debit), rows.Sum(r => r.Credit));
        return Ok(report);
    }
}

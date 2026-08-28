using AumoFinance.Api.Data;
using AumoFinance.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AumoFinance.Api.Controllers;

public record WorksheetRowDto(
    string AccountName,
    decimal TbDebit, decimal TbCredit,
    decimal AdjDebit, decimal AdjCredit,
    decimal AdjTbDebit, decimal AdjTbCredit,
    decimal IsDebit, decimal IsCredit,
    decimal BsDebit, decimal BsCredit
);

public record WorksheetTotalsDto(
    decimal TbDebit, decimal TbCredit,
    decimal AdjDebit, decimal AdjCredit,
    decimal AdjTbDebit, decimal AdjTbCredit,
    decimal IsDebit, decimal IsCredit,
    decimal BsDebit, decimal BsCredit,
    decimal NetIncome
);

public record WorksheetReportDto(List<WorksheetRowDto> Rows, WorksheetTotalsDto Totals);

[ApiController]
[Route("api/worksheet")]
[Authorize]
public class WorksheetController : ControllerBase
{
    private readonly AppDbContext _db;

    public WorksheetController(AppDbContext db)
    {
        _db = db;
    }

    // Response HARUS flat (field langsung di root row/totals, bukan dibungkus objek
    // "worksheet" bersarang) agar cocok dengan DTO Android.
    // Kolom: TB (jurnal General saja) -> Adj (jurnal Adjusting saja) -> Adjusted TB
    // (TB + Adj) -> IS (baris akun Revenue/Expense dari Adjusted TB) -> BS (baris
    // akun Asset/Liability/Equity dari Adjusted TB). Footer 3 baris standar:
    // Total (sebelum plug), NetIncome (plug ke BS), Total Akhir (setelah plug)
    // — NetIncome di totals dipakai frontend untuk merender baris ke-2 dan ke-3.
    [HttpGet]
    public async Task<IActionResult> GetWorksheet([FromQuery] int periodId)
    {
        var period = await _db.Periods.FirstOrDefaultAsync(p => p.Id == periodId);
        if (period == null) return NotFound(new { message = "Periode tidak ditemukan." });

        var accounts = await _db.Accounts.OrderBy(a => a.Code).ToListAsync();
        var lines = await _db.JournalLines
            .Include(l => l.JournalEntry)
            .Include(l => l.Account)
            .Where(l => l.JournalEntry.PeriodId == periodId && l.JournalEntry.Type != JournalType.Closing)
            .ToListAsync();

        var rows = new List<WorksheetRowDto>();
        foreach (var account in accounts)
        {
            var accLines = lines.Where(l => l.AccountId == account.Id).ToList();
            var tbDebit = accLines.Where(l => l.JournalEntry.Type == JournalType.General).Sum(l => l.Debit);
            var tbCredit = accLines.Where(l => l.JournalEntry.Type == JournalType.General).Sum(l => l.Credit);
            var adjDebit = accLines.Where(l => l.JournalEntry.Type == JournalType.Adjusting).Sum(l => l.Debit);
            var adjCredit = accLines.Where(l => l.JournalEntry.Type == JournalType.Adjusting).Sum(l => l.Credit);

            var netDebit = (tbDebit + adjDebit) - (tbCredit + adjCredit);
            var adjTbDebit = netDebit > 0 ? netDebit : 0m;
            var adjTbCredit = netDebit < 0 ? -netDebit : 0m;

            if (tbDebit == 0 && tbCredit == 0 && adjDebit == 0 && adjCredit == 0) continue;

            var isTemporary = account.Category is AccountCategory.Revenue or AccountCategory.Expense;
            rows.Add(new WorksheetRowDto(
                account.Name,
                tbDebit, tbCredit,
                adjDebit, adjCredit,
                adjTbDebit, adjTbCredit,
                isTemporary ? adjTbDebit : 0m, isTemporary ? adjTbCredit : 0m,
                isTemporary ? 0m : adjTbDebit, isTemporary ? 0m : adjTbCredit));
        }

        var netIncome = rows.Sum(r => r.IsCredit) - rows.Sum(r => r.IsDebit);

        var totals = new WorksheetTotalsDto(
            rows.Sum(r => r.TbDebit), rows.Sum(r => r.TbCredit),
            rows.Sum(r => r.AdjDebit), rows.Sum(r => r.AdjCredit),
            rows.Sum(r => r.AdjTbDebit), rows.Sum(r => r.AdjTbCredit),
            rows.Sum(r => r.IsDebit), rows.Sum(r => r.IsCredit),
            rows.Sum(r => r.BsDebit), rows.Sum(r => r.BsCredit),
            netIncome);

        return Ok(new WorksheetReportDto(rows, totals));
    }
}

using AumoFinance.Api.Data;
using AumoFinance.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AumoFinance.Api.Controllers;

public record ClosingJournalLineDto(string AccountName, decimal Debit, decimal Credit);
public record ClosingJournalReportDto(List<ClosingJournalLineDto> Entries);

[ApiController]
[Route("api/closingjournal")]
[Authorize]
public class ClosingJournalController : ControllerBase
{
    private readonly AppDbContext _db;

    public ClosingJournalController(AppDbContext db)
    {
        _db = db;
    }

    // Read-only. Entri Closing dibuat sistem saat periode ditutup (menutup akun
    // Temporary ke Retained Earnings) — TIDAK ADA endpoint POST/PUT/DELETE manual
    // untuk tipe ini (lihat larangan eksplisit di GeneralJournalController.Create).
    // TODO: PeriodsController.Close saat ini belum benar-benar men-generate entri
    // Closing ini secara otomatis — itu pekerjaan lanjutan berikutnya. Untuk saat
    // ini endpoint ini hanya membaca entri Closing yang SUDAH ada (jika ada).
    [HttpGet]
    public async Task<IActionResult> GetClosingJournal([FromQuery] int periodId)
    {
        var lines = await _db.JournalLines
            .Where(l => l.JournalEntry.PeriodId == periodId && l.JournalEntry.Type == JournalType.Closing)
            .Select(l => new ClosingJournalLineDto(l.Account.Name, l.Debit, l.Credit))
            .ToListAsync();

        return Ok(new ClosingJournalReportDto(lines));
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AumoFinance.Api.Controllers;

public record ClosingJournalLineDto(string AccountName, decimal Debit, decimal Credit);
public record ClosingJournalReportDto(List<ClosingJournalLineDto> Entries);

[ApiController]
[Route("api/closingjournal")]
[Authorize]
public class ClosingJournalController : ControllerBase
{
    // Read-only. Entri Closing dibuat sistem saat periode ditutup (menutup akun
    // Temporary ke Retained Earnings) — TIDAK ADA endpoint POST/PUT/DELETE manual
    // untuk tipe ini, sesuai aturan bisnis yang sudah ditetapkan di GeneralJournalController.
    // TODO fase berikutnya: generate entri closing nyata saat PeriodsController.Close dipanggil.
    [HttpGet]
    public IActionResult GetClosingJournal([FromQuery] int periodId)
    {
        return Ok(new ClosingJournalReportDto(new List<ClosingJournalLineDto>()));
    }
}

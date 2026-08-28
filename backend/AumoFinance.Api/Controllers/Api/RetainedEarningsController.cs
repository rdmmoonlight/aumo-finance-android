using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AumoFinance.Api.Controllers.Api;

public record RetainedEarningsReportDto(decimal BeginningBalance, decimal NetIncome, decimal Drawings, decimal EndingBalance);

[ApiController]
[Route("api/retainedearnings")]
[Authorize]
public class RetainedEarningsController : ControllerBase
{
    // Saldo Akhir = Saldo Awal + Laba Bersih - Prive.
    // TODO fase berikutnya: Saldo Awal diambil dari saldo Retained Earnings akhir
    // periode sebelumnya (carry-forward antar periode); Laba Bersih dari Income
    // Statement periode berjalan.
    [HttpGet]
    public IActionResult GetRetainedEarnings([FromQuery] int periodId)
    {
        return Ok(new RetainedEarningsReportDto(0m, 0m, 0m, 0m));
    }
}

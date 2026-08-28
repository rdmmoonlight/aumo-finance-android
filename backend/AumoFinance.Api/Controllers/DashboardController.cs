using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AumoFinance.Api.Controllers;

public record DashboardSummary(decimal TotalAssets, decimal TotalLiabilities, decimal TotalEquity, decimal NetIncome);

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    // TODO fase 4-6: hitung dari data jurnal/ledger nyata per periode (bukan hardcode).
    [HttpGet]
    public IActionResult GetSummary([FromQuery] int periodId)
    {
        var summary = new DashboardSummary(0m, 0m, 0m, 0m);
        return Ok(summary);
    }
}

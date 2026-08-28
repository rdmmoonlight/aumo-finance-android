using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AumoFinance.Api.Controllers.Api;

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
    // TODO fase berikutnya: derivasi dari mutasi akun Kas/Bank di General Ledger
    // dikelompokkan Operasi/Investasi/Pendanaan begitu EF Core terpasang.
    [HttpGet]
    public IActionResult GetCashFlow([FromQuery] int periodId)
    {
        var report = new CashFlowReportDto(
            new List<AccountAmountDto>(), 0m,
            new List<AccountAmountDto>(), 0m,
            new List<AccountAmountDto>(), 0m,
            0m, 0m);
        return Ok(report);
    }
}

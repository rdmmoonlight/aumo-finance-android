using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AumoFinance.Api.Controllers.Api;

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
    // Neraca: TotalAssets harus sama dengan TotalLiabilities + TotalEquity.
    // Sumber saldo akun Permanent bersifat kumulatif sejak awal (carry-forward
    // antar periode) — berbeda dengan General Ledger yang dibatasi ketat per periode.
    // TODO fase berikutnya: hitung dari akun Permanent nyata begitu EF Core terpasang.
    [HttpGet]
    public IActionResult GetFinancialPosition([FromQuery] int periodId)
    {
        var report = new FinancialPositionReportDto(
            new List<AccountAmountDto>(), 0m,
            new List<AccountAmountDto>(), 0m,
            new List<AccountAmountDto>(), 0m);
        return Ok(report);
    }
}

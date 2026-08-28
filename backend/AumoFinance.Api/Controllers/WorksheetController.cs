using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    // Response HARUS flat (field langsung di root row/totals, bukan dibungkus objek
    // "worksheet" bersarang) agar cocok dengan DTO Android — riwayat bug: bentuk
    // response bersarang membuat Android selalu mendeserialisasi list kosong.
    //
    // Footer WAJIB 3 baris total: Total (sebelum plug), Net Income plug, Total (setelah plug).
    // TODO fase berikutnya: hitung dari TrialBalanceController + Adjusting entries nyata
    // begitu EF Core terpasang.
    [HttpGet]
    public IActionResult GetWorksheet([FromQuery] int periodId)
    {
        var rows = new List<WorksheetRowDto>();
        var totals = new WorksheetTotalsDto(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        return Ok(new WorksheetReportDto(rows, totals));
    }
}

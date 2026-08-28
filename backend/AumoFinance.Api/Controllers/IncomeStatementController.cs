using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AumoFinance.Api.Controllers;

public record AccountAmountDto(string AccountName, decimal Amount);

public record IncomeStatementReportDto(
    List<AccountAmountDto> RevenueAccounts, decimal TotalRevenue,
    List<AccountAmountDto> ExpenseAccounts, decimal TotalExpenses,
    decimal OperatingIncome,
    List<AccountAmountDto> OtherIncomeAccounts,
    List<AccountAmountDto> OtherExpenseAccounts,
    decimal NetIncome
);

[ApiController]
[Route("api/incomestatement")]
[Authorize]
public class IncomeStatementController : ControllerBase
{
    // Response HARUS memakai nama field ini persis (revenueAccounts, expenseAccounts,
    // totalExpenses, operatingIncome, otherIncomeAccounts, otherExpenseAccounts) —
    // riwayat bug: response lama membungkus semua di objek "incomeStatement" dengan
    // nama field berbeda (revenues/operatingExpenses/totalOperatingExpenses), dan
    // operatingIncome/other* tidak pernah ada sama sekali di kontrak API, sehingga
    // Operating Income salah menampilkan nilai Net Income dan bagian Other selalu
    // tersembunyi secara hardcode.
    //
    // TODO fase berikutnya: hitung dari akun Temporary (Trial Balance jurnal
    // General+Adjusting) begitu EF Core terpasang.
    [HttpGet]
    public IActionResult GetIncomeStatement([FromQuery] int periodId)
    {
        var report = new IncomeStatementReportDto(
            new List<AccountAmountDto>(), 0m,
            new List<AccountAmountDto>(), 0m,
            0m,
            new List<AccountAmountDto>(),
            new List<AccountAmountDto>(),
            0m);
        return Ok(report);
    }
}

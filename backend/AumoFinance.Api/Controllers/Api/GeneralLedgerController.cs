using AumoFinance.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AumoFinance.Api.Controllers.Api;

public record LedgerLineDto(DateTime Date, string Description, decimal Debit, decimal Credit, decimal Balance);
public record LedgerAccountDto(int AccountId, string AccountName, List<LedgerLineDto> Lines, decimal EndingBalance);

[ApiController]
[Route("api/generalledger")]
[Authorize]
public class GeneralLedgerController : ControllerBase
{
    // TODO fase berikutnya: ganti sumber data ini dengan query EF Core nyata terhadap
    // Accounts + JournalEntries/JournalLines, begitu keduanya tersimpan lewat DbContext.
    //
    // PENTING (riwayat bug, jangan diulangi): General Ledger — baik Permanent maupun
    // Temporary — HARUS difilter ketat hanya pada transaksi di dalam rentang tanggal
    // periode yang dipilih (period.StartDate..period.EndDate). Jangan carry-over saldo
    // lintas periode di endpoint ini; carry-over saldo hanya berlaku untuk saldo awal
    // Neraca (opening balance), bukan untuk daftar mutasi ledger itu sendiri.
    [HttpGet]
    public IActionResult GetLedger([FromQuery] int periodId, [FromQuery] AccountType accountType)
    {
        var result = new List<LedgerAccountDto>();
        return Ok(result);
    }
}

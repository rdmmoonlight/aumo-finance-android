using AumoFinance.Api.Data;
using AumoFinance.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AumoFinance.Api.Controllers;

public record LedgerLineDto(DateTime Date, string Description, decimal Debit, decimal Credit, decimal Balance);
public record LedgerAccountDto(int AccountId, string AccountName, List<LedgerLineDto> Lines, decimal EndingBalance);

[ApiController]
[Route("api/generalledger")]
[Authorize]
public class GeneralLedgerController : ControllerBase
{
    private readonly AppDbContext _db;

    public GeneralLedgerController(AppDbContext db)
    {
        _db = db;
    }

    // PENTING (riwayat bug, jangan diulangi): General Ledger — baik Permanent maupun
    // Temporary — HARUS difilter ketat hanya pada transaksi di periode yang dipilih
    // (lewat PeriodId, bukan tanggal manual). Jangan carry-over saldo lintas periode
    // di endpoint ini; carry-over saldo hanya berlaku untuk saldo awal Neraca
    // (Financial Position), bukan untuk daftar mutasi ledger itu sendiri.
    // Closing DISERTAKAN (tidak difilter keluar): untuk akun Temporary, closing
    // entry itulah yang menutup saldo ke nol di akhir periode; untuk akun Permanent
    // (Retained Earnings), closing entry adalah mutasi nyata yang harus tampak di
    // ledger-nya. Mengecualikan Closing di sini akan membuat ledger tidak
    // mencerminkan posisi akun yang sebenarnya di akhir periode.
    [HttpGet]
    public async Task<IActionResult> GetLedger([FromQuery] int periodId, [FromQuery] AccountType accountType)
    {
        var period = await _db.Periods.FirstOrDefaultAsync(p => p.Id == periodId);
        if (period == null) return NotFound(new { message = "Periode tidak ditemukan." });

        var accounts = await _db.Accounts
            .Where(a => a.Type == accountType)
            .OrderBy(a => a.Code)
            .ToListAsync();

        var result = new List<LedgerAccountDto>();
        foreach (var account in accounts)
        {
            var lines = await _db.JournalLines
                .Include(l => l.JournalEntry)
                .Where(l => l.AccountId == account.Id && l.JournalEntry.PeriodId == periodId)
                .OrderBy(l => l.JournalEntry.EntryDate)
                .ThenBy(l => l.JournalEntry.CreatedAt)
                .ToListAsync();

            if (lines.Count == 0) continue;

            decimal runningBalance = 0m;
            var ledgerLines = new List<LedgerLineDto>();
            foreach (var line in lines)
            {
                runningBalance += line.Debit - line.Credit;
                ledgerLines.Add(new LedgerLineDto(
                    line.JournalEntry.EntryDate,
                    line.JournalEntry.TransactionNo,
                    line.Debit,
                    line.Credit,
                    runningBalance));
            }

            result.Add(new LedgerAccountDto(account.Id, account.Name, ledgerLines, runningBalance));
        }

        return Ok(result);
    }
}

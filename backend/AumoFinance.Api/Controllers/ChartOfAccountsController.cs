using AumoFinance.Api.Data;
using AumoFinance.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AumoFinance.Api.Controllers;

public record AccountRequest(string Code, string Name, AccountType Type, AccountCategory Category);
public record AccountDto(int Id, string Code, string Name, AccountType Type, AccountCategory Category, bool IsActive, decimal Balance);

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChartOfAccountsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ChartOfAccountsController(AppDbContext db)
    {
        _db = db;
    }

    // Balance dihitung sebagai (total Debit - total Kredit) dari seluruh JournalLine
    // akun ini. Catatan: ini net movement mentah, BELUM memperhitungkan sisi normal
    // per tipe akun (mis. akun Liabilitas/Ekuitas/Pendapatan idealnya ditampilkan
    // sebagai kredit-positif). Penyesuaian sisi normal menyusul di laporan
    // (TrialBalance/Ledger) yang sudah tahu klasifikasi akun secara detail.
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var accounts = await _db.Accounts
            .Select(a => new AccountDto(
                a.Id, a.Code, a.Name, a.Type, a.Category, a.IsActive,
                (a.Lines.Sum(l => (decimal?)l.Debit) ?? 0m) - (a.Lines.Sum(l => (decimal?)l.Credit) ?? 0m)))
            .OrderBy(a => a.Code)
            .ToListAsync();
        return Ok(accounts);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AccountRequest request)
    {
        if (await _db.Accounts.AnyAsync(a => a.Code == request.Code))
            return Conflict(new { message = $"Kode akun '{request.Code}' sudah dipakai." });

        var account = new Account { Code = request.Code, Name = request.Name, Type = request.Type, Category = request.Category };
        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();
        return Ok(new AccountDto(account.Id, account.Code, account.Name, account.Type, account.Category, account.IsActive, 0m));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] AccountRequest request)
    {
        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == id);
        if (account == null) return NotFound();

        if (await _db.Accounts.AnyAsync(a => a.Code == request.Code && a.Id != id))
            return Conflict(new { message = $"Kode akun '{request.Code}' sudah dipakai." });

        account.Code = request.Code;
        account.Name = request.Name;
        account.Type = request.Type;
        account.Category = request.Category;
        await _db.SaveChangesAsync();
        return Ok(account);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == id);
        if (account == null) return NotFound();

        var hasTransactions = await _db.JournalLines.AnyAsync(l => l.AccountId == id);
        if (hasTransactions)
        {
            // Jangan hapus permanen akun yang sudah punya mutasi jurnal — bisa merusak
            // laporan historis. Nonaktifkan saja.
            account.IsActive = false;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        _db.Accounts.Remove(account);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

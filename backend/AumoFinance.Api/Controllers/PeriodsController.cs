using AumoFinance.Api.Data;
using AumoFinance.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AumoFinance.Api.Controllers;

public record OpenPeriodRequest(string Name, DateTime StartDate, DateTime EndDate, decimal? OpeningCashBalance);

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PeriodsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PeriodsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var periods = await _db.Periods.OrderBy(p => p.StartDate).ToListAsync();
        return Ok(periods);
    }

    [HttpPost]
    public async Task<IActionResult> Open([FromBody] OpenPeriodRequest request)
    {
        var period = new Period
        {
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsClosed = false,
            IsSelected = true
        };

        var others = await _db.Periods.ToListAsync();
        foreach (var p in others) p.IsSelected = false;

        _db.Periods.Add(period);
        await _db.SaveChangesAsync();

        // TODO: jika OpeningCashBalance diisi dan tidak ada akun permanen sebelumnya,
        // posting jurnal umum pembukaan (Debit Kas/Bank, Kredit Modal Awal) tanggal awal periode.

        return Ok(period);
    }

    [HttpPut("{id}/close")]
    public async Task<IActionResult> Close(int id)
    {
        var period = await _db.Periods.FirstOrDefaultAsync(p => p.Id == id);
        if (period == null) return NotFound();
        period.IsClosed = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

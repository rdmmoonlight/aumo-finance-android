using AumoFinance.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AumoFinance.Api.Controllers;

public record OpenPeriodRequest(string Name, DateTime StartDate, DateTime EndDate, decimal? OpeningCashBalance);

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PeriodsController : ControllerBase
{
    // TODO fase berikutnya: ganti in-memory list ini dengan EF Core + DbContext nyata.
    private static readonly List<Period> _periods = new();
    private static int _nextId = 1;

    [HttpGet]
    public IActionResult List() => Ok(_periods);

    [HttpPost]
    public IActionResult Open([FromBody] OpenPeriodRequest request)
    {
        var period = new Period
        {
            Id = _nextId++,
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsClosed = false,
            IsSelected = true
        };
        foreach (var p in _periods) p.IsSelected = false;
        _periods.Add(period);

        // TODO: jika OpeningCashBalance diisi dan tidak ada akun permanen sebelumnya,
        // posting jurnal umum pembukaan (Debit Kas/Bank, Kredit Modal Awal) tanggal awal periode.

        return Ok(period);
    }

    [HttpPut("{id}/close")]
    public IActionResult Close(int id)
    {
        var period = _periods.FirstOrDefault(p => p.Id == id);
        if (period == null) return NotFound();
        period.IsClosed = true;
        return NoContent();
    }
}

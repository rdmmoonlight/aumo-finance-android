using AumoFinance.Api.Data;
using AumoFinance.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AumoFinance.Api.Controllers;

public record JournalLineRequest(int AccountId, decimal Debit, decimal Credit);
public record JournalEntryRequest(int PeriodId, DateTime EntryDate, DateTime CreatedAt, JournalType Type, List<JournalLineRequest> Lines);

[ApiController]
[Route("api/generaljournal")]
[Authorize]
public class GeneralJournalController : ControllerBase
{
    private readonly AppDbContext _db;

    public GeneralJournalController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int periodId)
    {
        var entries = await _db.JournalEntries
            .Include(e => e.Lines).ThenInclude(l => l.Account)
            .Where(e => e.PeriodId == periodId)
            .OrderBy(e => e.EntryDate)
            .ToListAsync();
        return Ok(entries);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] JournalEntryRequest request)
    {
        // Hanya tipe General dan Adjusting yang boleh diinput manual di halaman ini;
        // Closing bersifat system-generated (lihat PeriodsController.Close nanti).
        if (request.Type == JournalType.Closing)
            return BadRequest(new { message = "Jurnal Closing tidak boleh diinput manual." });

        var period = await _db.Periods.FirstOrDefaultAsync(p => p.Id == request.PeriodId);
        if (period == null)
            return BadRequest(new { message = "Periode tidak ditemukan." });
        if (period.IsClosed)
            return BadRequest(new { message = "Periode ini sudah ditutup, tidak bisa menambah entri baru." });

        var lines = request.Lines.Select(l => new JournalLine { AccountId = l.AccountId, Debit = l.Debit, Credit = l.Credit }).ToList();
        if (lines.Sum(l => l.Debit) != lines.Sum(l => l.Credit))
            return BadRequest(new { message = "Entri tidak balance: total debit harus sama dengan total kredit." });

        var entry = new JournalEntry
        {
            PeriodId = request.PeriodId,
            EntryDate = DateTime.SpecifyKind(request.EntryDate, DateTimeKind.Unspecified),
            CreatedAt = DateTime.SpecifyKind(request.CreatedAt, DateTimeKind.Unspecified),
            Type = request.Type,
            Lines = lines
        };
        _db.JournalEntries.Add(entry);
        await _db.SaveChangesAsync();

        // Nomor transaksi diisi setelah Id nyata didapat dari database.
        entry.TransactionNo = $"JE-{entry.Id:D5}";
        await _db.SaveChangesAsync();

        return Ok(entry);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] JournalEntryRequest request)
    {
        var entry = await _db.JournalEntries.Include(e => e.Lines).FirstOrDefaultAsync(e => e.Id == id);
        if (entry == null) return NotFound();

        var period = await _db.Periods.FirstOrDefaultAsync(p => p.Id == entry.PeriodId);
        if (period != null && period.IsClosed)
            return BadRequest(new { message = "Periode ini sudah ditutup, tidak bisa mengubah entri." });

        var newLines = request.Lines.Select(l => new JournalLine { AccountId = l.AccountId, Debit = l.Debit, Credit = l.Credit }).ToList();
        if (newLines.Sum(l => l.Debit) != newLines.Sum(l => l.Credit))
            return BadRequest(new { message = "Entri tidak balance: total debit harus sama dengan total kredit." });

        entry.EntryDate = DateTime.SpecifyKind(request.EntryDate, DateTimeKind.Unspecified);
        entry.Type = request.Type;
        _db.JournalLines.RemoveRange(entry.Lines);
        entry.Lines = newLines;

        await _db.SaveChangesAsync();
        return Ok(entry);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entry = await _db.JournalEntries.FirstOrDefaultAsync(e => e.Id == id);
        if (entry == null) return NotFound();
        _db.JournalEntries.Remove(entry);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Laporan untuk halaman General Journal / Adjusting Journal.
    // Hanya menampilkan entri sesuai "type" yang diminta (General atau Adjusting)
    // DAN dibatasi ke periode yang dipilih — Closing tidak pernah muncul di sini
    // karena bersifat system-generated.
    [HttpGet("report")]
    public async Task<IActionResult> GetReport([FromQuery] int periodId, [FromQuery] JournalType type)
    {
        if (type == JournalType.Closing)
            return BadRequest(new { message = "Laporan Closing tidak tersedia di halaman ini." });

        var result = await _db.JournalEntries
            .Include(e => e.Lines).ThenInclude(l => l.Account)
            .Where(e => e.PeriodId == periodId && e.Type == type)
            .OrderBy(e => e.EntryDate)
            .ToListAsync();

        return Ok(result);
    }
}

using AumoFinance.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AumoFinance.Api.Controllers.Api;

public record JournalLineRequest(int AccountId, decimal Debit, decimal Credit);
public record JournalEntryRequest(DateTime EntryDate, DateTime CreatedAt, JournalType Type, List<JournalLineRequest> Lines);

[ApiController]
[Route("api/generaljournal")]
[Authorize]
public class GeneralJournalController : ControllerBase
{
    // TODO fase berikutnya: ganti in-memory list ini dengan EF Core + DbContext nyata,
    // dan validasi periode tidak dalam status closed sebelum menerima entri baru.
    private static readonly List<JournalEntry> _entries = new();
    private static int _nextId = 1;

    [HttpGet]
    public IActionResult List() => Ok(_entries);

    [HttpPost]
    public IActionResult Create([FromBody] JournalEntryRequest request)
    {
        // Hanya tipe General dan Adjusting yang boleh diinput manual di halaman ini;
        // Closing bersifat system-generated.
        if (request.Type == JournalType.Closing)
            return BadRequest(new { message = "Jurnal Closing tidak boleh diinput manual." });

        var entry = new JournalEntry
        {
            Id = _nextId++,
            TransactionNo = $"JE-{_nextId:D5}",
            EntryDate = DateTime.SpecifyKind(request.EntryDate, DateTimeKind.Unspecified),
            CreatedAt = DateTime.SpecifyKind(request.CreatedAt, DateTimeKind.Unspecified),
            Type = request.Type,
            Lines = request.Lines.Select(l => new JournalLine { AccountId = l.AccountId, Debit = l.Debit, Credit = l.Credit }).ToList()
        };
        _entries.Add(entry);
        return Ok(entry);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] JournalEntryRequest request)
    {
        var entry = _entries.FirstOrDefault(e => e.Id == id);
        if (entry == null) return NotFound();
        entry.EntryDate = DateTime.SpecifyKind(request.EntryDate, DateTimeKind.Unspecified);
        entry.Type = request.Type;
        entry.Lines = request.Lines.Select(l => new JournalLine { AccountId = l.AccountId, Debit = l.Debit, Credit = l.Credit }).ToList();
        return Ok(entry);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var entry = _entries.FirstOrDefault(e => e.Id == id);
        if (entry == null) return NotFound();
        _entries.Remove(entry);
        return NoContent();
    }

    // Laporan untuk halaman General Journal / Adjusting Journal.
    // Hanya menampilkan entri sesuai "type" yang diminta (General atau Adjusting) —
    // Closing tidak pernah muncul di sini karena bersifat system-generated.
    // TODO fase berikutnya: filter juga berdasarkan rentang tanggal periode (periodId)
    // begitu Period tersimpan lewat EF Core, bukan lagi in-memory.
    [HttpGet("report")]
    public IActionResult GetReport([FromQuery] int periodId, [FromQuery] JournalType type)
    {
        if (type == JournalType.Closing)
            return BadRequest(new { message = "Laporan Closing tidak tersedia di halaman ini." });

        var result = _entries
            .Where(e => e.Type == type)
            .OrderBy(e => e.EntryDate)
            .ToList();

        return Ok(result);
    }
}

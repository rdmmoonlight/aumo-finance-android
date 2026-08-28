using AumoFinance.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AumoFinance.Api.Controllers.Api;

public record TrialBalanceRowDto(int AccountId, string AccountName, decimal Debit, decimal Credit);
public record TrialBalanceReportDto(List<TrialBalanceRowDto> Rows, decimal TotalDebit, decimal TotalCredit);

[ApiController]
[Route("api/trialbalance")]
[Authorize]
public class TrialBalanceController : ControllerBase
{
    // ATURAN BISNIS (final, jangan diubah tanpa instruksi eksplisit):
    // - Trial Balance TIDAK disesuaikan (adjusted=false): hanya menghitung jurnal type=General.
    // - Adjusted Trial Balance (adjusted=true): menghitung jurnal type=General + Adjusting.
    // - Jurnal type=Closing TIDAK PERNAH dihitung di kedua varian ini.
    // - Semua akun (Permanent maupun Temporary) dibatasi ketat pada rentang tanggal
    //   periode yang dipilih (period.StartDate..EndDate) — TIDAK ada carry-over
    //   kumulatif lintas periode untuk akun Permanent di laporan ini.
    // - Bandingkan EntryDate.Date terhadap StartDate.Date/EndDate.Date untuk menghindari
    //   mismatch waktu/DateTimeKind (riwayat bug: Adjusted TB pernah identik dgn Unadjusted TB).
    //
    // TODO fase berikutnya: ganti dengan query EF Core nyata terhadap Accounts + JournalEntries
    // begitu Period tersimpan lewat DbContext (saat ini Period masih in-memory di PeriodsController).
    [HttpGet]
    public IActionResult GetTrialBalance([FromQuery] int periodId, [FromQuery] bool adjusted)
    {
        var allowedTypes = adjusted
            ? new[] { JournalType.General, JournalType.Adjusting }
            : new[] { JournalType.General };

        // Placeholder kosong sampai data periode & jurnal nyata tersedia dari EF Core.
        var rows = new List<TrialBalanceRowDto>();
        var report = new TrialBalanceReportDto(rows, rows.Sum(r => r.Debit), rows.Sum(r => r.Credit));

        return Ok(report);
    }
}

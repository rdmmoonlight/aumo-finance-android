using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace AumoFinance.Services.Reports;

public class ClosingJournalService : BaseApiService
{
    // ==========================================
    // GET CLOSING JOURNAL REPORT
    // ==========================================
    public async Task<(ClosingJournalReportApiResponse? data, string? errorDetail)> GetClosingJournalReportAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Get, "/api/mobile/reports/closing-journal");

            using var response = await HttpClient.SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ClosingJournalReportApiResponse>(content, JsonOptions);
                return (result, null);
            }

            var snippet = content.Length > 150 ? content[..150] : content;
            return (null, $"HTTP {(int)response.StatusCode} ({response.StatusCode}) — {snippet}");
        }
        catch (TaskCanceledException)
        {
            return (null, "Timeout — server tidak merespons dalam 15 detik (kemungkinan cold start Railway).");
        }
        catch (Exception ex)
        {
            return (null, $"{ex.GetType().Name}: {ex.Message}");
        }
    }
}

// ==========================================
// DTO / MODEL RESPONSE CLOSING JOURNAL
// ==========================================
public class ClosingJournalReportApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("hasPeriodSelected")]
    public bool HasPeriodSelected { get; set; }

    [JsonPropertyName("selectedPeriodName")]
    public string? SelectedPeriodName { get; set; }

    [JsonPropertyName("entries")]
    public List<ClosingJournalEntryDto> Entries { get; set; } = new();
}

public class ClosingJournalEntryDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("referenceNumber")]
    public string? ReferenceNumber { get; set; }

    [JsonPropertyName("entryDate")]
    public DateTime EntryDate { get; set; }

    [JsonPropertyName("journalType")]
    public string? JournalType { get; set; }

    [JsonPropertyName("lines")]
    public List<ClosingJournalLineDto> Lines { get; set; } = new();

    public decimal TotalDebit => Lines.Sum(l => l.Debit);
    public decimal TotalCredit => Lines.Sum(l => l.Credit);
}

public class ClosingJournalLineDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("accountId")]
    public int AccountId { get; set; }

    [JsonPropertyName("accountName")]
    public string? AccountName { get; set; }

    [JsonPropertyName("referenceNumber")]
    public int ReferenceNumber { get; set; }

    [JsonPropertyName("lineDescription")]
    public string? LineDescription { get; set; }

    [JsonPropertyName("debit")]
    public decimal Debit { get; set; }

    [JsonPropertyName("credit")]
    public decimal Credit { get; set; }
}

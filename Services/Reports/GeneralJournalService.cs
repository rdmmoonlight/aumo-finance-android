using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace AumoFinance.Services.Reports;

public class GeneralJournalService : BaseApiService
{
    // ==========================================
    // GET GENERAL JOURNAL REPORT
    // ==========================================
    public async Task<(GeneralJournalReportApiResponse? data, string? errorDetail)> GetGeneralJournalReportAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Get, "/api/mobile/journal-entries");

            using var response = await HttpClient.SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<GeneralJournalReportApiResponse>(content, JsonOptions);
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
// DTO / MODEL RESPONSE GENERAL JOURNAL
// ==========================================
public class GeneralJournalReportApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("hasPeriodSelected")]
    public bool HasPeriodSelected { get; set; }

    [JsonPropertyName("selectedPeriodName")]
    public string? SelectedPeriodName { get; set; }

    [JsonPropertyName("isPeriodClosed")]
    public bool IsPeriodClosed { get; set; }

    [JsonPropertyName("entries")]
    public List<GeneralJournalEntryReportDto> Entries { get; set; } = new();
}

public class GeneralJournalEntryReportDto
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
    public List<GeneralJournalLineReportDto> Lines { get; set; } = new();

    public decimal TotalDebit => Lines.Sum(l => l.Debit);
    public decimal TotalCredit => Lines.Sum(l => l.Credit);
}

public class GeneralJournalLineReportDto
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

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace AumoFinance.Services;

public class PeriodService : BaseApiService
{
    // ==========================================
    // 1. GET ALL PERIODS
    // ==========================================
    public async Task<(List<PeriodApiModel> periods, int? selectedPeriodId, string? errorDetail)> GetPeriodsAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Get, "/api/mobile/periods");

            using var response = await HttpClient.SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var envelope = JsonSerializer.Deserialize<PeriodsEnvelopeApiResponse>(content, JsonOptions);
                return (envelope?.Periods ?? new(), envelope?.SelectedPeriodId, null);
            }

            var snippet = content.Length > 150 ? content[..150] : content;
            return (new List<PeriodApiModel>(), null, $"HTTP {(int)response.StatusCode} ({response.StatusCode}) — {snippet}");
        }
        catch (TaskCanceledException)
        {
            return (new List<PeriodApiModel>(), null, "Timeout — server tidak merespons dalam 15 detik (kemungkinan cold start Railway).");
        }
        catch (Exception ex)
        {
            return (new List<PeriodApiModel>(), null, $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    // ==========================================
    // 2. SELECT PERIOD (SET ACTIVE VIEW)
    // ==========================================
    public async Task<(bool success, string message)> SelectPeriodAsync(int periodId)
    {
        return await PostPeriodActionAsync($"/api/mobile/periods/select/{periodId}");
    }

    // ==========================================
    // 3. CLEAR PERIOD SELECTION
    // ==========================================
    public async Task<(bool success, string message)> ClearPeriodSelectionAsync()
    {
        return await PostPeriodActionAsync("/api/mobile/periods/clear-selection");
    }

    // ==========================================
    // 4. CLOSE PERIOD
    // ==========================================
    public async Task<(bool success, string message)> ClosePeriodAsync(int periodId)
    {
        return await PostPeriodActionAsync($"/api/mobile/periods/close/{periodId}");
    }

    // ==========================================
    // 5. CREATE NEW PERIOD
    // ==========================================
    public async Task<(bool success, string message)> CreatePeriodAsync(string periodName, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(periodName))
        {
            return (false, "Nama periode wajib diisi.");
        }

        if (startDate >= endDate)
        {
            return (false, "Tanggal mulai harus sebelum tanggal selesai.");
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Post, "/api/mobile/periods/create");

            var payload = new
            {
                periodName = periodName.Trim(),
                startDate = startDate,
                endDate = endDate
            };
            request.Content = JsonContent.Create(payload);

            using var response = await HttpClient.SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);
            var apiRes = JsonSerializer.Deserialize<PeriodActionApiResponse>(content, JsonOptions);

            if (response.IsSuccessStatusCode)
            {
                return (true, apiRes?.Message ?? "Periode berhasil dibuka.");
            }

            return (false, apiRes?.Message ?? $"HTTP {(int)response.StatusCode} ({response.StatusCode}).");
        }
        catch (TaskCanceledException)
        {
            return (false, "Timeout — server tidak merespons dalam 15 detik (kemungkinan cold start Railway).");
        }
        catch (Exception ex)
        {
            return (false, $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    // Helper bersama untuk aksi POST periode (select/clear/close)
    private async Task<(bool success, string message)> PostPeriodActionAsync(string requestUri)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Post, requestUri);

            using var response = await HttpClient.SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);
            var apiRes = JsonSerializer.Deserialize<PeriodActionApiResponse>(content, JsonOptions);

            if (response.IsSuccessStatusCode)
            {
                return (true, apiRes?.Message ?? "Berhasil.");
            }

            return (false, apiRes?.Message ?? $"HTTP {(int)response.StatusCode} ({response.StatusCode}).");
        }
        catch (TaskCanceledException)
        {
            return (false, "Timeout — server tidak merespons dalam 15 detik (kemungkinan cold start Railway).");
        }
        catch (Exception ex)
        {
            return (false, $"{ex.GetType().Name}: {ex.Message}");
        }
    }
}

// ==========================================
// DTO / MODEL RESPONSE PERIOD
// ==========================================
public class PeriodsEnvelopeApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("selectedPeriodId")]
    public int? SelectedPeriodId { get; set; }

    [JsonPropertyName("periods")]
    public List<PeriodApiModel> Periods { get; set; } = new();
}

public class PeriodApiModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("periodName")]
    public string PeriodName { get; set; } = string.Empty;

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }

    [JsonPropertyName("isClosed")]
    public bool IsClosed { get; set; }

    [JsonPropertyName("closedAt")]
    public DateTime? ClosedAt { get; set; }

    [JsonPropertyName("isSelected")]
    public bool IsSelected { get; set; }
}

public class PeriodActionApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

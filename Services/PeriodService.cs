using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace AumoFinance.Services;

public class PeriodService : BaseApiService
{
    private const string BaseEndpoint = "/api/mobile/periods";

    public async Task<(List<PeriodApiModel>? periods, string? selectedPeriodId, string? errorDetail)> GetPeriodsAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Get, BaseEndpoint);

            using var response = await HttpClient.SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<GetPeriodsApiResponse>(content, JsonOptions);
                return (result?.Periods ?? new(), result?.SelectedPeriodId?.ToString(), null);
            }

            var snippet = content.Length > 150 ? content[..150] : content;
            return (null, null, $"HTTP {(int)response.StatusCode} — {snippet}");
        }
        catch (Exception ex)
        {
            return (null, null, $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    public async Task<(bool success, string message)> SelectPeriodAsync(string periodId)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Post, $"{BaseEndpoint}/select/{periodId}");

            using var response = await HttpClient.SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<BasicPeriodResponse>(content, JsonOptions);
                return (true, result?.Message ?? "Period selected successfully.");
            }

            var errResult = JsonSerializer.Deserialize<BasicPeriodResponse>(content, JsonOptions);
            return (false, errResult?.Message ?? $"HTTP {(int)response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    public async Task<(bool success, string message)> ClosePeriodAsync(int id)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Post, $"{BaseEndpoint}/close/{id}");

            using var response = await HttpClient.SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<BasicPeriodResponse>(content, JsonOptions);
                return (true, result?.Message ?? "Period closed successfully.");
            }

            var errResult = JsonSerializer.Deserialize<BasicPeriodResponse>(content, JsonOptions);
            return (false, errResult?.Message ?? $"HTTP {(int)response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    public async Task<(bool success, string message)> ReopenPeriodAsync(int id)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Post, $"{BaseEndpoint}/reopen/{id}");

            using var response = await HttpClient.SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<BasicPeriodResponse>(content, JsonOptions);
                return (true, result?.Message ?? "Period reopened successfully.");
            }

            var errResult = JsonSerializer.Deserialize<BasicPeriodResponse>(content, JsonOptions);
            return (false, errResult?.Message ?? $"HTTP {(int)response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    public async Task<(bool success, string message)> CreatePeriodAsync(string name, DateTime startDate, DateTime endDate)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Post, $"{BaseEndpoint}/create");

            var dto = new CreatePeriodRequest { PeriodName = name, StartDate = startDate, EndDate = endDate };
            var jsonBody = JsonSerializer.Serialize(dto, JsonOptions);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            using var response = await HttpClient.SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<BasicPeriodResponse>(content, JsonOptions);
                return (true, result?.Message ?? "Period created successfully.");
            }

            var errResult = JsonSerializer.Deserialize<BasicPeriodResponse>(content, JsonOptions);
            return (false, errResult?.Message ?? $"HTTP {(int)response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, $"{ex.GetType().Name}: {ex.Message}");
        }
    }
}

public class GetPeriodsApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("selectedPeriodId")]
    public int? SelectedPeriodId { get; set; }

    [JsonPropertyName("periods")]
    public List<PeriodApiModel> Periods { get; set; } = new();
}

public class BasicPeriodResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
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

    [JsonPropertyName("isSelected")]
    public bool IsSelected { get; set; }

    public string DateRangeDisplay => $"{StartDate:MMM dd, yyyy} - {EndDate:MMM dd, yyyy}";
    public bool CanSelect => !IsSelected;
    public bool CanClose => !IsClosed;
}

public class CreatePeriodRequest
{
    [JsonPropertyName("periodName")]
    public string PeriodName { get; set; } = string.Empty;

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }
}

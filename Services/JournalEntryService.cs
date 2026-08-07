using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace AumoFinance.Services;

public class JournalEntryService : BaseApiService
{
    private const string BaseEndpoint = "/api/mobile/journal-entry";

    // ==========================================
    // 1. GET BY ID: /api/mobile/journal-entry/{id}
    // ==========================================
    public async Task<(JournalEntryDetailDto? entry, string? errorDetail)> GetJournalEntryByIdAsync(int id)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Get, $"{BaseEndpoint}/{id}");

            using var response = await HttpClient.SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<GetJournalEntryApiResponse>(content, JsonOptions);
                return (result?.Entry, null);
            }

            var snippet = content.Length > 150 ? content[..150] : content;
            return (null, $"HTTP {(int)response.StatusCode} — {snippet}");
        }
        catch (TaskCanceledException)
        {
            return (null, "Timeout — server did not respond within 15 seconds.");
        }
        catch (Exception ex)
        {
            return (null, $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    // ==========================================
    // 2. CREATE: /api/mobile/journal-entry/create
    // ==========================================
    public async Task<(bool success, string message, int entryId, string referenceNumber)> CreateJournalEntryAsync(CreateJournalEntryRequest dto)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Post, $"{BaseEndpoint}/create");

            var jsonBody = JsonSerializer.Serialize(dto, JsonOptions);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            using var response = await HttpClient.SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<CreateJournalEntryApiResponse>(content, JsonOptions);
                return (true, result?.Message ?? "Journal entry created successfully.", result?.EntryId ?? 0, result?.ReferenceNumber ?? string.Empty);
            }

            var errResult = JsonSerializer.Deserialize<BasicApiResponse>(content, JsonOptions);
            string message = !string.IsNullOrEmpty(errResult?.Message) ? errResult.Message : $"HTTP {(int)response.StatusCode}";
            return (false, message, 0, string.Empty);
        }
        catch (TaskCanceledException)
        {
            return (false, "Timeout — server did not respond within 15 seconds.", 0, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, $"{ex.GetType().Name}: {ex.Message}", 0, string.Empty);
        }
    }

    // ==========================================
    // 3. EDIT: /api/mobile/journal-entry/edit/{id}
    // ==========================================
    public async Task<(bool success, string message)> EditJournalEntryAsync(int id, UpdateJournalEntryRequest dto)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Put, $"{BaseEndpoint}/edit/{id}");

            var jsonBody = JsonSerializer.Serialize(dto, JsonOptions);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            using var response = await HttpClient.SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<BasicApiResponse>(content, JsonOptions);
                return (true, result?.Message ?? "Journal entry updated successfully.");
            }

            var errResult = JsonSerializer.Deserialize<BasicApiResponse>(content, JsonOptions);
            string message = !string.IsNullOrEmpty(errResult?.Message) ? errResult.Message : $"HTTP {(int)response.StatusCode}";
            return (false, message);
        }
        catch (TaskCanceledException)
        {
            return (false, "Timeout — server did not respond within 15 seconds.");
        }
        catch (Exception ex)
        {
            return (false, $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    // ==========================================
    // 4. DELETE: /api/mobile/journal-entry/delete/{id}
    // ==========================================
    public async Task<(bool success, string message)> DeleteJournalEntryAsync(int id)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Delete, $"{BaseEndpoint}/delete/{id}");

            using var response = await HttpClient.SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<BasicApiResponse>(content, JsonOptions);
                return (true, result?.Message ?? "Journal entry deleted successfully.");
            }

            var errResult = JsonSerializer.Deserialize<BasicApiResponse>(content, JsonOptions);
            string message = !string.IsNullOrEmpty(errResult?.Message) ? errResult.Message : $"HTTP {(int)response.StatusCode}";
            return (false, message);
        }
        catch (TaskCanceledException)
        {
            return (false, "Timeout — server did not respond within 15 seconds.");
        }
        catch (Exception ex)
        {
            return (false, $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    // ==========================================
    // 5. SEARCH DESCRIPTIONS: /api/mobile/journal-entry/search-descriptions?q=xxx
    // ==========================================
    public async Task<List<string>> SearchDescriptionsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
            return new List<string>();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            string encodedQuery = Uri.EscapeDataString(query.Trim());
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Get, $"{BaseEndpoint}/search-descriptions?q={encodedQuery}");

            using var response = await HttpClient.SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var suggestions = JsonSerializer.Deserialize<List<string>>(content, JsonOptions);
                return suggestions ?? new List<string>();
            }

            return new List<string>();
        }
        catch (Exception)
        {
            return new List<string>();
        }
    }
}

// ==========================================
// REQUEST DTOs
// ==========================================
public class CreateJournalEntryRequest
{
    [JsonPropertyName("journalType")]
    public string JournalType { get; set; } = "General";

    [JsonPropertyName("entryDate")]
    public DateTime EntryDate { get; set; } = DateTime.Today;

    [JsonPropertyName("lines")]
    public List<JournalEntryLineRequest> Lines { get; set; } = new();
}

public class UpdateJournalEntryRequest
{
    [JsonPropertyName("journalType")]
    public string JournalType { get; set; } = "General";

    [JsonPropertyName("entryDate")]
    public DateTime EntryDate { get; set; } = DateTime.Today;

    [JsonPropertyName("lines")]
    public List<JournalEntryLineRequest> Lines { get; set; } = new();
}

public class JournalEntryLineRequest
{
    [JsonPropertyName("accountId")]
    public int AccountId { get; set; }

    [JsonPropertyName("lineDescription")]
    public string? LineDescription { get; set; }

    [JsonPropertyName("debit")]
    public decimal Debit { get; set; }

    [JsonPropertyName("credit")]
    public decimal Credit { get; set; }
}

// ==========================================
// RESPONSE DTOs
// ==========================================
public class BasicApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class CreateJournalEntryApiResponse : BasicApiResponse
{
    [JsonPropertyName("entryId")]
    public int EntryId { get; set; }

    [JsonPropertyName("referenceNumber")]
    public string? ReferenceNumber { get; set; }
}

public class GetJournalEntryApiResponse : BasicApiResponse
{
    [JsonPropertyName("entry")]
    public JournalEntryDetailDto? Entry { get; set; }
}

public class JournalEntryDetailDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("referenceNumber")]
    public string ReferenceNumber { get; set; } = string.Empty;

    [JsonPropertyName("journalType")]
    public string JournalType { get; set; } = string.Empty;

    [JsonPropertyName("entryDate")]
    public DateTime EntryDate { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("isLocked")]
    public bool IsLocked { get; set; }

    [JsonPropertyName("lines")]
    public List<JournalEntryLineDetailDto> Lines { get; set; } = new();
}

public class JournalEntryLineDetailDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("accountId")]
    public int AccountId { get; set; }

    [JsonPropertyName("lineDescription")]
    public string? LineDescription { get; set; }

    [JsonPropertyName("debit")]
    public decimal Debit { get; set; }

    [JsonPropertyName("credit")]
    public decimal Credit { get; set; }

    [JsonPropertyName("lineOrder")]
    public int LineOrder { get; set; }
}

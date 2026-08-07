using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using AumoFinance.Models;

namespace AumoFinance.Services;

// DTO untuk pembuatan jurnal
public class CreateJournalDto
{
    public DateTime EntryDate { get; set; } = DateTime.Now;
    public string? JournalType { get; set; } = "General";
    public List<CreateJournalLineDto>? Lines { get; set; } = new();
}

public class CreateJournalLineDto
{
    public int AccountId { get; set; }
    public string? LineDescription { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

public class ApiService
{
    // Public Base URL of your ASP.NET Core Web Backend
    public const string BaseUrl = "https://aumo-preview.up.railway.app";

    private static readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri(BaseUrl),
        Timeout = TimeSpan.FromSeconds(15)
    };

    private const string AuthTokenKey = "auth_token_jwt";

    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    // Helper untuk membuat HttpRequestMessage dengan Auth Header yang thread-safe
    private async Task<HttpRequestMessage> CreateAuthenticatedRequestAsync(HttpMethod method, string requestUri)
    {
        var request = new HttpRequestMessage(method, requestUri);
        var token = await SecureStorage.Default.GetAsync(AuthTokenKey);

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return request;
    }

    // ==========================================
    // 1. LOGIN
    // ==========================================
    public async Task<(bool success, string message, string? userId)> LoginAsync(string usernameOrEmail, string password)
    {
        if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Username/Email dan password wajib diisi.", null);
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var payload = new { email = usernameOrEmail, password = password };

            var response = await _httpClient.PostAsJsonAsync("/api/mobile/auth/login", payload, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);

            var result = JsonSerializer.Deserialize<MobileLoginResponse>(content, _jsonOptions);

            if (response.IsSuccessStatusCode && result != null && result.Success)
            {
                if (!string.IsNullOrEmpty(result.Token))
                {
                    await SecureStorage.Default.SetAsync(AuthTokenKey, result.Token);
                }
                return (true, result.Message ?? "Login berhasil.", result.UserId);
            }

            return (false, result?.Message ?? "Email/Username atau password salah.", null);
        }
        catch (TaskCanceledException)
        {
            return (false, "Koneksi RTO (Timeout). Periksa koneksi internet Anda.", null);
        }
        catch (Exception ex)
        {
            return (false, $"Gagal terhubung ke server: {ex.Message}", null);
        }
    }

    // ==========================================
    // 2. GET ACCOUNTS (ringkas — untuk dropdown Journal Entry)
    // ==========================================
    public async Task<List<AccountLookupModel>> GetAccountsAsync()
    {
        var (accounts, _, _) = await GetChartOfAccountsFullAsync();
        return accounts.Where(a => a.IsActive).Select(a => new AccountLookupModel
        {
            Id = a.Id,
            AccountName = a.AccountName,
            ReferenceNumber = a.ReferenceNumber
        }).ToList();
    }

    // ==========================================
    // 2b. GET CHART OF ACCOUNTS (detail penuh — untuk CoaPage)
    // ==========================================
    public async Task<(List<CoaApiModel> data, string? selectedPeriodName, string? errorDetail)> GetChartOfAccountsFullAsync(string? search = null, string? category = null)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
            if (!string.IsNullOrWhiteSpace(category)) query.Add($"category={Uri.EscapeDataString(category)}");
            string uri = "/api/mobile/chart-of-accounts" + (query.Count > 0 ? "?" + string.Join("&", query) : "");

            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Get, uri);
            using var response = await _httpClient.SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var envelope = JsonSerializer.Deserialize<CoaEnvelopeModel>(content, _jsonOptions);
                return (envelope?.Accounts ?? new(), envelope?.SelectedPeriodName, null);
            }

            var snippet = content.Length > 150 ? content[..150] : content;
            return (new List<CoaApiModel>(), null, $"HTTP {(int)response.StatusCode} ({response.StatusCode}) — {snippet}");
        }
        catch (TaskCanceledException)
        {
            return (new List<CoaApiModel>(), null, "Timeout — server tidak merespons dalam 15 detik (kemungkinan cold start Railway).");
        }
        catch (Exception ex)
        {
            return (new List<CoaApiModel>(), null, $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    // ==========================================
    // 2c. DELETE ACCOUNT
    // ==========================================
    public async Task<(bool success, string message)> DeleteAccountAsync(int accountId)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Delete, $"/api/mobile/chart-of-accounts/delete/{accountId}");

            using var response = await _httpClient.SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);
            var apiRes = JsonSerializer.Deserialize<ApiResponseModel>(content, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                return (true, apiRes?.Message ?? "Akun berhasil dihapus.");
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

    // ==========================================
    // 3. POST JOURNAL ENTRY
    // ==========================================
    public async Task<(bool success, string message)> PostJournalAsync(CreateJournalDto dto)
    {
        var lines = dto.Lines?.FindAll(l => l.AccountId != 0 && (l.Debit != 0 || l.Credit != 0)) ?? new();
        if (lines.Count < 2)
        {
            return (false, "Jurnal harus memiliki minimal 2 baris rincian (line items).");
        }

        var totalDebit = 0m;
        var totalCredit = 0m;
        foreach (var l in lines)
        {
            totalDebit += l.Debit;
            totalCredit += l.Credit;
        }

        if (totalDebit != totalCredit || totalDebit == 0)
        {
            return (false, $"Total Debit (Rp{totalDebit:N0}) dan Kredit (Rp{totalCredit:N0}) harus sama dan tidak boleh nol.");
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Post, "/api/mobile/journal-entries");

            var payload = new
            {
                entryDate = dto.EntryDate,
                journalType = string.IsNullOrWhiteSpace(dto.JournalType) ? "General" : dto.JournalType,
                lines = lines
            };

            request.Content = JsonContent.Create(payload);

            using var response = await _httpClient.SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);

            var apiRes = JsonSerializer.Deserialize<ApiResponseModel>(content, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                return (true, apiRes?.Message ?? "Jurnal berhasil disimpan.");
            }
            else
            {
                return (false, apiRes?.Message ?? "Gagal menyimpan jurnal ke server.");
            }
        }
        catch (TaskCanceledException)
        {
            return (false, "Koneksi RTO (15 detik). Periksa jaringan Anda.");
        }
        catch (Exception ex)
        {
            return (false, $"Terjadi kesalahan: {ex.Message}");
        }
    }

    // ==========================================
    // 3b. GET GENERAL JOURNAL LIST
    // ==========================================
    public async Task<(List<JournalEntryDisplayModel> data, string? selectedPeriodName, bool isPeriodClosed, string? errorDetail)> GetGeneralJournalAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Get, "/api/mobile/journal-entries");

            using var response = await _httpClient.SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var envelope = JsonSerializer.Deserialize<GeneralJournalEnvelopeModel>(content, _jsonOptions);
                var mapped = (envelope?.Entries ?? new()).Select(e => new JournalEntryDisplayModel
                {
                    Id = e.Id,
                    EntryDate = e.EntryDate,
                    Lines = e.Lines.Select(l => new JournalEntryLineDisplayModel
                    {
                        AccountName = l.AccountName,
                        RefNumber = l.ReferenceNumber.ToString(),
                        LineDescription = l.LineDescription,
                        Debit = l.Debit,
                        Credit = l.Credit
                    }).ToList()
                }).ToList();

                return (mapped, envelope?.SelectedPeriodName, envelope?.IsPeriodClosed ?? false, null);
            }

            var snippet = content.Length > 150 ? content[..150] : content;
            return (new(), null, false, $"HTTP {(int)response.StatusCode} ({response.StatusCode}) — {snippet}");
        }
        catch (TaskCanceledException)
        {
            return (new(), null, false, "Timeout — server tidak merespons dalam 15 detik (kemungkinan cold start Railway).");
        }
        catch (Exception ex)
        {
            return (new(), null, false, $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    // ==========================================
    // 4. SEARCH DESCRIPTIONS (AUTO-COMPLETE)
    // ==========================================
    public async Task<List<string>> SearchDescriptionsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
            return new List<string>();

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var encodedQuery = Uri.EscapeDataString(query.Trim());
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Get, $"/api/mobile/journal-entries/search-descriptions?q={encodedQuery}");

            using var response = await _httpClient.SendAsync(request, cts.Token);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cts.Token);
                return JsonSerializer.Deserialize<List<string>>(content, _jsonOptions) ?? new List<string>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SearchDescriptionsAsync Error: {ex.Message}");
        }

        return new List<string>();
    }

    // ==========================================
    // 5. GET DASHBOARD DATA
    // ==========================================
    // Mengembalikan (data, errorDetail) agar penyebab kegagalan bisa
    // ditampilkan langsung di UI (tidak bergantung logcat/PC tools).
    public async Task<(object? data, string? errorDetail)> GetDashboardAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Get, "/api/mobile/dashboard");

            using var response = await _httpClient.SendAsync(request, cts.Token);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cts.Token);
                return (JsonSerializer.Deserialize<object>(content, _jsonOptions), null);
            }

            // Status non-sukses (mis. 401 auth, 404, 500) — bukan masalah koneksi.
            var body = await response.Content.ReadAsStringAsync(cts.Token);
            var snippet = body.Length > 150 ? body[..150] : body;
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

    // ==========================================
    // 6. LOGOUT
    // ==========================================
    public void Logout()
    {
        SecureStorage.Default.Remove(AuthTokenKey);
    }

    // ==========================================
    // 7. GET PERIODS
    // ==========================================
    public async Task<(List<PeriodApiModel> data, int? selectedPeriodId, string? errorDetail)> GetPeriodsAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Get, "/api/mobile/periods");

            using var response = await _httpClient.SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var envelope = JsonSerializer.Deserialize<PeriodsEnvelopeModel>(content, _jsonOptions);
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
    // 8. SELECT PERIOD (VIEW)
    // ==========================================
    public async Task<(bool success, string message)> SelectPeriodAsync(int periodId)
    {
        return await PostPeriodActionAsync($"/api/mobile/periods/select/{periodId}");
    }

    // ==========================================
    // 9. CLEAR PERIOD SELECTION (STOP VIEWING)
    // ==========================================
    public async Task<(bool success, string message)> ClearPeriodSelectionAsync()
    {
        return await PostPeriodActionAsync("/api/mobile/periods/clear-selection");
    }

    // ==========================================
    // 10. CLOSE PERIOD
    // ==========================================
    public async Task<(bool success, string message)> ClosePeriodAsync(int periodId)
    {
        return await PostPeriodActionAsync($"/api/mobile/periods/close/{periodId}");
    }

    // ==========================================
    // 11. CREATE (OPEN) NEW PERIOD
    // ==========================================
    public async Task<(bool success, string message)> CreatePeriodAsync(string periodName, DateTime startDate, DateTime endDate)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Post, "/api/mobile/periods/create");

            var payload = new { periodName, startDate, endDate };
            request.Content = JsonContent.Create(payload);

            using var response = await _httpClient.SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);
            var apiRes = JsonSerializer.Deserialize<ApiResponseModel>(content, _jsonOptions);

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

    // Helper bersama untuk aksi POST periode (select/clear/close) yang polanya identik.
    private async Task<(bool success, string message)> PostPeriodActionAsync(string requestUri)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Post, requestUri);

            using var response = await _httpClient.SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);
            var apiRes = JsonSerializer.Deserialize<ApiResponseModel>(content, _jsonOptions);

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

    // Internal DTO models for JSON Deserialization
    private class MobileLoginResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("userId")]
        public string? UserId { get; set; }
    }

    private class ApiResponseModel
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}

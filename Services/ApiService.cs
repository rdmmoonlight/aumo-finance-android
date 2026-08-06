using System;
using System.Collections.Generic;
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

public class ApiService
{
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

            var response = await _httpClient.PostAsJsonAsync("/api/mobile/login", payload, cts.Token);
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
    // 2. GET ACCOUNTS
    // ==========================================
    public async Task<List<AccountLookupModel>> GetAccountsAsync()
    {
        var result = new List<AccountLookupModel>();
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Get, "/api/mobile/accounts");

            using var response = await _httpClient.SendAsync(request, cts.Token);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cts.Token);
                var accounts = JsonSerializer.Deserialize<List<AccountLookupModel>>(content, _jsonOptions);
                if (accounts != null)
                {
                    result = accounts;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetAccountsAsync Error: {ex.Message}");
        }

        return result;
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
            using var request = await CreateAuthenticatedRequestAsync(HttpMethod.Get, $"/api/mobile/search-descriptions?q={encodedQuery}");

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
    // 5. LOGOUT
    // ==========================================
    public void Logout()
    {
        SecureStorage.Default.Remove(AuthTokenKey);
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

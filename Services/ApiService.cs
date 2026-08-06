using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Storage; // Adjust if using Xamarin.Essentials / Plugin.SecureStorage
using AumoFinance.Models;

namespace AumoFinance.Services;

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

    // Helper to attach Bearer Token to HTTP Request Headers
    private async Task SetAuthorizationHeaderAsync()
    {
        var token = await SecureStorage.Default.GetAsync(AuthTokenKey);
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    // ==========================================
    // 1. LOGIN
    // ==========================================
    public async Task<(bool success, string message, string? userId)> LoginAsync(string usernameOrEmail, string password)
    {
        if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Username/Email and password are required.", null);
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
                // Securely save JWT token on device
                await SecureStorage.Default.SetAsync(AuthTokenKey, result.Token);
                return (true, result.Message ?? "Login successful.", result.UserId);
            }

            return (false, result?.Message ?? "Invalid email/username or password.", null);
        }
        catch (TaskCanceledException)
        {
            return (false, "Connection timed out. Please check your internet connection.", null);
        }
        catch (Exception ex)
        {
            return (false, $"Connection error: {ex.Message}", null);
        }
    }

    // ==========================================
    // 2. GET ACCOUNTS (Chart of Accounts)
    // ==========================================
    public async Task<List<AccountLookupModel>> GetAccountsAsync()
    {
        var result = new List<AccountLookupModel>();
        try
        {
            await SetAuthorizationHeaderAsync();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

            var response = await _httpClient.GetAsync("/api/mobile/accounts", cts.Token);
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
            System.Diagnostics.Debug.WriteLine($"GetAccountsAsync failed: {ex.Message}");
        }

        return result;
    }

    // ==========================================
    // 3. POST JOURNAL ENTRY
    // ==========================================
    public async Task<(bool success, string message)> PostJournalAsync(CreateJournalDto dto)
    {
        var lines = dto.Lines.FindAll(l => l.AccountId != 0 && (l.Debit != 0 || l.Credit != 0));
        if (lines.Count < 2)
        {
            return (false, "A journal entry must have at least two line items.");
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
            return (false, $"Total Debit (${totalDebit:N2}) and Credit (${totalCredit:N2}) must be equal.");
        }

        try
        {
            await SetAuthorizationHeaderAsync();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

            var payload = new
            {
                entryDate = dto.EntryDate,
                journalType = "General",
                mobileNote = dto.Note,
                lines = lines
            };

            var response = await _httpClient.PostAsJsonAsync("/api/mobile/journal-entries", payload, cts.Token);
            var content = await response.Content.ReadAsStringAsync(cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var apiRes = JsonSerializer.Deserialize<ApiResponseModel>(content, _jsonOptions);
                return (true, apiRes?.Message ?? "Journal entry posted successfully.");
            }
            else
            {
                var errRes = JsonSerializer.Deserialize<ApiResponseModel>(content, _jsonOptions);
                return (false, errRes?.Message ?? "Failed to post journal entry to server.");
            }
        }
        catch (TaskCanceledException)
        {
            return (false, "Connection timed out (15s). Please check your network.");
        }
        catch (Exception ex)
        {
            return (false, $"An error occurred: {ex.Message}");
        }
    }

    // ==========================================
    // 4. LOGOUT
    // ==========================================
    public void Logout()
    {
        SecureStorage.Default.Remove(AuthTokenKey);
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    // Internal DTO models for handling JSON responses
    private class MobileLoginResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public string? UserId { get; set; }
    }

    private class ApiResponseModel
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}

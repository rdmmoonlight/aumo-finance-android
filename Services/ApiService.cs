using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Storage; // Sesuaikan jika menggunakan Xamarin.Essentials / Plugin.SecureStorage
using AumoFinance.Models;

namespace AumoFinance.Services;

public class ApiService
{
    // Base URL Backend Web ASP.NET Core Anda (misal Railway / VPS / Localhost)
    // Silakan ganti sesuai URL publik server web Anda
    public const string BaseUrl = "https://aumo-finance-web.up.railway.app"; 

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

    // Helper untuk memasang Bearer Token pada Header Request HTTP
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
            return (false, "Username/Email dan password harus diisi.", null);
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
                // Simpan token JWT dengan aman di perangkat
                await SecureStorage.Default.SetAsync(AuthTokenKey, result.Token);
                return (true, result.Message ?? "Login berhasil.", result.UserId);
            }

            return (false, result?.Message ?? "Email/Username atau password salah.", null);
        }
        catch (TaskCanceledException)
        {
            return (false, "Koneksi ke server timeout. Periksa koneksi internet Anda.", null);
        }
        catch (Exception ex)
        {
            return (false, $"Terjadi kesalahan koneksi: {ex.Message}", null);
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
            System.Diagnostics.Debug.WriteLine($"GetAccountsAsync gagal: {ex.Message}");
        }

        return result;
    }

    // ==========================================
    // 3. POST JOURNAL (Simpan Transaksi Jurnal)
    // ==========================================
    public async Task<(bool success, string message)> PostJournalAsync(CreateJournalDto dto)
    {
        var lines = dto.Lines.FindAll(l => l.AccountId != 0 && (l.Debit != 0 || l.Credit != 0));
        if (lines.Count < 2)
        {
            return (false, "Jurnal harus memiliki minimal dua baris.");
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
            return (false, $"Total Debit (Rp {totalDebit:N0}) dan Kredit (Rp {totalCredit:N0}) harus seimbang.");
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
                return (true, apiRes?.Message ?? "Jurnal berhasil disimpan.");
            }
            else
            {
                var errRes = JsonSerializer.Deserialize<ApiResponseModel>(content, _jsonOptions);
                return (false, errRes?.Message ?? "Gagal menyimpan jurnal ke server.");
            }
        }
        catch (TaskCanceledException)
        {
            return (false, "Koneksi ke server timeout (15 detik). Cek jaringan internet Anda.");
        }
        catch (Exception ex)
        {
            return (false, $"Terjadi kesalahan: {ex.Message}");
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

    // Model DTO internal pendukung penanganan response JSON
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

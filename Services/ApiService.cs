using System.Net.Http.Json;
using AumoFinance.Models;

namespace AumoFinance.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "https://aumo.up.railway.app/api/mobile/";

    public ApiService()
    {
        _http = new HttpClient 
        { 
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    public async Task<DashboardModel?> GetDashboardAsync()
    {
        try { return await _http.GetFromJsonAsync<DashboardModel>("dashboard"); }
        catch { return null; }
    }

    // Ambil daftar akun
    public async Task<List<AccountLookupModel>> GetAccountsAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<AccountLookupModel>>("accounts") ?? new();
        }
        catch { return new(); }
    }

    // Post Jurnal Baru
    public async Task<(bool success, string message)> PostJournalAsync(CreateJournalDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("journal", dto);
            if (response.IsSuccessStatusCode)
            {
                return (true, "Jurnal berhasil disimpan!");
            }
            var err = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            return (false, err != null && err.ContainsKey("message") ? err["message"].ToString()! : "Gagal menyimpan jurnal.");
        }
        catch (Exception ex)
        {
            return (false, $"Error koneksi: {ex.Message}");
        }
    }
}

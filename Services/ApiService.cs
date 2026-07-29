using System.Net.Http.Json;
using AumoFinance.Models; // <-- Ubah dari Aumo.Models ke AumoFinance.Models

namespace AumoFinance.Services; // <-- Ubah dari Aumo.Services ke AumoFinance.Services

public class ApiService
{
    private readonly HttpClient _http;
    
    // Kunci langsung ke Production Domain Railway
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
        try
        {
            return await _http.GetFromJsonAsync<DashboardModel>("dashboard");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API Error] {ex.Message}");
            return null;
        }
    }
}

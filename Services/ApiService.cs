using System.Net.Http.Json;
using AumoFinance.Models;

namespace AumoFinance.Services;

public class ApiService
{
    private readonly HttpClient _http;
    
    // Ganti dengan URL domain/IP Backend Web Anda (misal: http://10.0.2.2:5000/api/mobile/ jika local emulator)
    private const string BaseUrl = "https://aumo.up.railway.app/api/mobile/";

    public ApiService()
    {
        _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    public async Task<DashboardModel?> GetDashboardAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<DashboardModel>("dashboard");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching dashboard: {ex.Message}");
            return null;
        }
    }
}

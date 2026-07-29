using System.Net.Http.Json;

namespace AumoFinance.Services;

public class ApiService
{
    private readonly HttpClient _http;
    
    // Ganti dengan URL domain/IP Backend Web Anda
    private const string BaseUrl = "https://api.aumofinance.com/api/mobile/";

    public ApiService()
    {
        _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    public async Task<MobileDashboardResponse?> GetDashboardAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<MobileDashboardResponse>("dashboard");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> SaveJournalAsync(DateTime date, decimal amount, string desc)
    {
        try
        {
            var payload = new { Date = date, Amount = amount, Description = desc };
            var response = await _http.PostAsJsonAsync("journal", payload);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

public record MobileDashboardResponse(decimal TotalCash, string ActivePeriod);

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace AumoFinance.Services;

public abstract class BaseApiService
{
    public const string BaseUrl = "https://aumo.onrender.com";
    protected const string AuthTokenKey = "auth_token_jwt";

    protected static readonly HttpClient HttpClient = new HttpClient
    {
        BaseAddress = new Uri(BaseUrl),
        Timeout = TimeSpan.FromSeconds(15)
    };

    protected static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    // Helper membuat Request bertoken Bearer
    protected async Task<HttpRequestMessage> CreateAuthenticatedRequestAsync(HttpMethod method, string requestUri)
    {
        var request = new HttpRequestMessage(method, requestUri);
        var token = await SecureStorage.Default.GetAsync(AuthTokenKey);

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return request;
    }
}

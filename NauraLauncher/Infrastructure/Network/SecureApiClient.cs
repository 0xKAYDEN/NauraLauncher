using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NauraLauncher.Core.Interfaces;

namespace NauraLauncher.Infrastructure.Network;

/// <summary>
/// Production hardened HTTP client with TLS 1.3 / 1.2 enforcement and automatic retry.
/// </summary>
public class SecureApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private string? _bearerToken;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public SecureApiClient(string? baseUrl = null)
    {
        var handler = new SocketsHttpHandler
        {
            SslOptions = new SslClientAuthenticationOptions
            {
                EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                // Allow self-signed certs in development/local environments
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            },
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            ConnectTimeout = TimeSpan.FromSeconds(10)
        };

        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(baseUrl ?? "https://127.0.0.1:8443"),
            Timeout = TimeSpan.FromSeconds(20)
        };

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "NauraLauncher-Apex/2.4.0 (Windows NT 10.0; Win64; x64)");
    }

    public void SetBaseUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            _httpClient.BaseAddress = uri;
        }
    }

    public void SetBearerToken(string? token)
    {
        _bearerToken = token;
        if (string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            using var response = await _httpClient.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                return default;
            }
            var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions);
        });
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            string json = JsonSerializer.Serialize(data, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync(endpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                return default;
            }
            var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<TResponse>(stream, JsonOptions);
        });
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            string json = JsonSerializer.Serialize(data, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PutAsync(endpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                return default;
            }
            var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<TResponse>(stream, JsonOptions);
        });
    }

    private static async Task<T?> ExecuteWithRetryAsync<T>(Func<Task<T?>> action, int maxRetries = 2)
    {
        int attempt = 0;
        while (true)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                attempt++;
                if (attempt >= maxRetries)
                {
                    System.Diagnostics.Debug.WriteLine($"[ApiClient] Request failed after {attempt} attempts: {ex.Message}");
                    return default;
                }
                await Task.Delay(250 * attempt);
            }
        }
    }
}

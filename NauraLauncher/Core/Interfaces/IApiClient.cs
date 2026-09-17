using System.Threading.Tasks;

namespace NauraLauncher.Core.Interfaces;

public interface IApiClient
{
    void SetBaseUrl(string url);
    void SetBearerToken(string? token);

    Task<T?> GetAsync<T>(string endpoint);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data);
    Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data);
}

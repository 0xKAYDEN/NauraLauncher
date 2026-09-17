using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using NauraLauncher.Core.Entities;
using NauraLauncher.Core.Interfaces;

namespace NauraLauncher.Infrastructure.Services;

/// <summary>
/// Production settings service managing persistent local JSON config and cloud synchronization.
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly IApiClient _apiClient;
    private readonly string _settingsFilePath;

    public LauncherConfig Config { get; private set; } = new();

    public SettingsService(IApiClient apiClient)
    {
        _apiClient = apiClient;
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string folder = Path.Combine(appData, "NauraLauncher");
        Directory.CreateDirectory(folder);
        _settingsFilePath = Path.Combine(folder, "settings.json");

        _ = LoadAsync();
    }

    public async Task LoadAsync()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                string json = await File.ReadAllTextAsync(_settingsFilePath);
                var loaded = JsonSerializer.Deserialize<LauncherConfig>(json);
                if (loaded != null)
                {
                    Config = loaded;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsService] Load error: {ex.Message}");
        }
    }

    public async Task SaveAsync()
    {
        try
        {
            string json = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsFilePath, json);
            _ = SyncCloudAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SettingsService] Save error: {ex.Message}");
        }
    }

    public async Task SyncCloudAsync()
    {
        try
        {
            await _apiClient.PutAsync<LauncherConfig, object>("/api/v1/settings", Config);
        }
        catch { }
    }
}

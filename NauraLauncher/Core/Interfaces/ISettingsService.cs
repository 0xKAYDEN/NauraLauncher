using System.Threading.Tasks;
using NauraLauncher.Core.Entities;

namespace NauraLauncher.Core.Interfaces;

public interface ISettingsService
{
    LauncherConfig Config { get; }
    Task LoadAsync();
    Task SaveAsync();
    Task SyncCloudAsync();
}

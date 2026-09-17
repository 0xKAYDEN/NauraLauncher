using System.Collections.Generic;
using System.Threading.Tasks;
using NauraLauncher.Models;

namespace NauraLauncher.Core.Interfaces;

public interface IGameLibraryService
{
    Task<IReadOnlyList<LibraryItem>> GetUserLibraryAsync();
    Task<IReadOnlyList<GameEntry>> GetCatalogAsync(string? category = null);
    Task<GameEntry?> GetFeaturedGameAsync();
    Task<bool> ClaimOrPurchaseGameAsync(string gameId);
    Task RecordPlaytimeAsync(string gameId, int seconds);
}

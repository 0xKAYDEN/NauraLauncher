using NauraLauncher.Application.Interfaces;
using NauraLauncher.Domain.Entities;

namespace NauraLauncher.Application.Services;

public class InventoryService : IInventoryService
{
    private static readonly Dictionary<int, List<InventoryItem>> _inventories = new();
    private static int _nextItemId = 1000;

    static InventoryService()
    {
        // Seed inventories for mock users
        var users = AuthService.GetAllUsers();
        foreach (var user in users)
        {
            var items = MockDataService.GenerateMockInventory(user.Id);
            _inventories[user.Id] = items;
            _nextItemId = Math.Max(_nextItemId, items.Max(i => i.Id) + 1);
        }
    }

    public Task<List<InventoryItem>> GetUserInventoryAsync(int userId)
    {
        if (!_inventories.TryGetValue(userId, out var inv))
        {
            inv = MockDataService.GenerateMockInventory(userId);
            _inventories[userId] = inv;
        }
        return Task.FromResult(inv.ToList());
    }

    public Task<InventoryItem?> GetItemByIdAsync(int itemId)
    {
        foreach (var inv in _inventories.Values)
        {
            var item = inv.FirstOrDefault(i => i.Id == itemId);
            if (item != null) return Task.FromResult<InventoryItem?>(item);
        }
        return Task.FromResult<InventoryItem?>(null);
    }

    public Task<InventoryItem> AddItemAsync(int userId, InventoryItem item)
    {
        if (!_inventories.ContainsKey(userId))
            _inventories[userId] = new List<InventoryItem>();

        item.Id = _nextItemId++;
        item.UserId = userId;
        item.AcquiredAt = DateTime.UtcNow;
        _inventories[userId].Add(item);
        return Task.FromResult(item);
    }

    public Task<bool> RemoveItemAsync(int userId, int itemId)
    {
        if (!_inventories.TryGetValue(userId, out var inv)) return Task.FromResult(false);
        var item = inv.FirstOrDefault(i => i.Id == itemId);
        if (item == null) return Task.FromResult(false);
        inv.Remove(item);
        return Task.FromResult(true);
    }

    public Task<bool> UpdateItemAsync(InventoryItem item)
    {
        if (!_inventories.TryGetValue(item.UserId, out var inv)) return Task.FromResult(false);
        var existing = inv.FirstOrDefault(i => i.Id == item.Id);
        if (existing == null) return Task.FromResult(false);
        var idx = inv.IndexOf(existing);
        inv[idx] = item;
        return Task.FromResult(true);
    }

    public Task<bool> TransferItemAsync(int fromUserId, int toUserId, int itemId)
    {
        if (!_inventories.TryGetValue(fromUserId, out var fromInv)) return Task.FromResult(false);
        var item = fromInv.FirstOrDefault(i => i.Id == itemId);
        if (item == null) return Task.FromResult(false);

        fromInv.Remove(item);
        item.UserId = toUserId;
        if (!_inventories.ContainsKey(toUserId))
            _inventories[toUserId] = new List<InventoryItem>();
        _inventories[toUserId].Add(item);
        return Task.FromResult(true);
    }

    public async Task<List<InventoryItem>> GetTradableItemsAsync(int userId)
    {
        var inv = await GetUserInventoryAsync(userId);
        return inv.Where(i => i.IsTradable && !i.IsBound && !i.IsLocked).ToList();
    }

    // For other services
    public static Dictionary<int, List<InventoryItem>> GetAllInventories() => _inventories;
}

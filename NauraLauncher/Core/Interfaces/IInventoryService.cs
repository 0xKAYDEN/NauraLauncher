using NauraLauncher.Domain.Entities;

namespace NauraLauncher.Core.Interfaces;

public interface IInventoryService
{
    Task<List<InventoryItem>> GetUserInventoryAsync(int userId);
    Task<InventoryItem?> GetItemByIdAsync(int itemId);
    Task<InventoryItem> AddItemAsync(int userId, InventoryItem item);
    Task<bool> RemoveItemAsync(int userId, int itemId);
    Task<bool> UpdateItemAsync(InventoryItem item);
    Task<bool> TransferItemAsync(int fromUserId, int toUserId, int itemId);
    Task<List<InventoryItem>> GetTradableItemsAsync(int userId);
}

public interface IWalletService
{
    Task<Wallet> GetWalletAsync(int userId);
    Task<bool> AddCurrencyAsync(int userId, Domain.Enums.CurrencyType type, long amount);
    Task<bool> DeductCurrencyAsync(int userId, Domain.Enums.CurrencyType type, long amount);
    Task<bool> TransferCurrencyAsync(int fromUserId, int toUserId, Domain.Enums.CurrencyType type, long amount);
}

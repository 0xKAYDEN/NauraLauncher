using NauraLauncher.Core.Interfaces;
using NauraLauncher.Domain.Entities;
using NauraLauncher.Domain.Enums;

namespace NauraLauncher.Core.Services;

public class WalletService : IWalletService
{
    public Task<Wallet> GetWalletAsync(int userId)
    {
        var user = AuthService.GetAllUsers().FirstOrDefault(u => u.Id == userId);
        if (user == null) return Task.FromResult(new Wallet { UserId = userId });
        return Task.FromResult(user.Wallet);
    }

    public async Task<bool> AddCurrencyAsync(int userId, CurrencyType type, long amount)
    {
        var wallet = await GetWalletAsync(userId);
        wallet.Add(type, amount);
        return true;
    }

    public async Task<bool> DeductCurrencyAsync(int userId, CurrencyType type, long amount)
    {
        var wallet = await GetWalletAsync(userId);
        return wallet.TryDeduct(type, amount);
    }

    public async Task<bool> TransferCurrencyAsync(int fromUserId, int toUserId, CurrencyType type, long amount)
    {
        var fromWallet = await GetWalletAsync(fromUserId);
        if (!fromWallet.TryDeduct(type, amount)) return false;
        var toWallet = await GetWalletAsync(toUserId);
        toWallet.Add(type, amount);
        return true;
    }
}

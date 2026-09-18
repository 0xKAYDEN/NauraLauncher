using System.Collections.ObjectModel;
using NauraLauncher.Common;
using NauraLauncher.Domain.Entities;
using NauraLauncher.Domain.Enums;

namespace NauraLauncher.ViewModels;

public class ProfileViewModel : ObservableObject
{
    private User? _user;
    public User? User
    {
        get => _user;
        set
        {
            if (SetProperty(ref _user, value))
            {
                OnPropertyChanged(nameof(DisplayName));
                OnPropertyChanged(nameof(Username));
                OnPropertyChanged(nameof(LevelDisplay));
                OnPropertyChanged(nameof(ClassDisplay));
                OnPropertyChanged(nameof(RebornDisplay));
                OnPropertyChanged(nameof(AvatarPath));
                OnPropertyChanged(nameof(CpsDisplay));
                OnPropertyChanged(nameof(GoldDisplay));
                OnPropertyChanged(nameof(SilverDisplay));
                OnPropertyChanged(nameof(StatusMessage));
            }
        }
    }

    public string DisplayName => User?.DisplayName ?? "Unknown Hero";
    public string Username => User?.Username ?? "guest";
    public string LevelDisplay => User != null ? $"Lv. {User.Level}" : "Lv. 1";
    public string ClassDisplay => User?.MainClass.ToDisplayName() ?? "Trojan";
    public string RebornDisplay => User?.Reborn switch
    {
        RebornStage.FirstReborn => "1st Reborn",
        RebornStage.SecondReborn => "2nd Reborn",
        _ => "No Reborn"
    };
    public string AvatarPath => User?.AvatarPath ?? "Assets/avatar_conquer.png";
    public string StatusMessage => User?.StatusMessage ?? "Offline";
    public string CpsDisplay => User?.Wallet.Cps.ToString("N0") ?? "0";
    public string GoldDisplay => User?.Wallet.Gold.ToString("N0") ?? "0";
    public string SilverDisplay => User?.Wallet.Silver.ToString("N0") ?? "0";
    public string BoundCpsDisplay => User?.Wallet.BoundCps.ToString("N0") ?? "0";

    public string CpsFull => $"{CpsDisplay} CPs";
    public string GoldFull => $"{GoldDisplay} Gold";
    public string SilverFull => $"{SilverDisplay} Silver";

    // Stats
    public ObservableCollection<StatRow> Stats { get; } = new();

    public ProfileViewModel()
    {
        // Example stats
        Stats.Add(new StatRow { Label = "PLAYTIME THIS WEEK", Value = "23H 40M", IconKey = "Icon.Clock" });
        Stats.Add(new StatRow { Label = "MONSTERS SLAIN", Value = "12,482", IconKey = "Icon.Sword" });
        Stats.Add(new StatRow { Label = "PK WINS", Value = "87", IconKey = "Icon.Trophy" });
    }

    public void Refresh()
    {
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(Username));
        OnPropertyChanged(nameof(LevelDisplay));
        OnPropertyChanged(nameof(ClassDisplay));
        OnPropertyChanged(nameof(RebornDisplay));
        OnPropertyChanged(nameof(AvatarPath));
        OnPropertyChanged(nameof(CpsDisplay));
        OnPropertyChanged(nameof(GoldDisplay));
        OnPropertyChanged(nameof(SilverDisplay));
        OnPropertyChanged(nameof(CpsFull));
        OnPropertyChanged(nameof(GoldFull));
        OnPropertyChanged(nameof(SilverFull));
        OnPropertyChanged(nameof(BoundCpsDisplay));
        OnPropertyChanged(nameof(StatusMessage));
    }
}

public class StatRow
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string IconKey { get; set; } = string.Empty;
}

# NauraLauncher

APEX launcher shell for Windows — WPF / .NET 8, no third-party dependencies.

## Structure

The app is a single frameless window that acts as a **page shell**: the header
(logo, centered nav pill, search, credits, user, window buttons) and the status
bar are persistent, and the content area is swapped based on the selected nav
tab.

```
MainWindow.xaml            shell chrome + page host (16px rounded frame)
Views/HomePage.xaml        greeting, quick stats, Continue Playing, Latest Intel, library
Views/MarketplacePage.xaml spotlight hero, category chips, curated archives
Views/AuctionPage.xaml     rarity filters, live lot + countdown, bid panel, vault drops
Views/SettingsPage.xaml    sections rail, Switch / Slider / segmented control groups
Themes/Colors.xaml         shadcn "zinc" dark design tokens
Themes/Typography.xaml     display / body / label text styles
Themes/Icons.xaml          Lucide-style 24x24 outline geometry
Themes/Controls.xaml       shadcn component library (Button, Card, Badge, Input,
                           Switch, Slider, Segmented, Progress, ScrollBar)
Themes/Converters.xaml     shared IValueConverter instances
```

`MainViewModel` is a shell coordinator: it owns the `Home` / `Marketplace` /
`Auction` / `Settings` sub view models and exposes `IsHome`, `IsMarketplace`,
`IsAuction` and `IsSettings` flags that each page binds its `Visibility` to.

```xaml
<Grid Visibility="{Binding IsAuction, Converter={StaticResource BoolVis}}">
    <views:AuctionPage DataContext="{Binding Auction}" />
</Grid>
```

## Build

```
dotnet build NauraLauncher/NauraLauncher.csproj
dotnet run   --project NauraLauncher/NauraLauncher.csproj
```

Requires the .NET 8 SDK on Windows (WPF is a Windows-only workload).

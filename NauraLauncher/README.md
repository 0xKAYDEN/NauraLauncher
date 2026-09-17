# NauraLauncher — “APEX” Marketplace Shell (WPF)

A pixel-faithful **WPF (.NET 8)** conversion of the supplied **APEX** game‑marketplace
launcher design, built with an MVVM architecture and a hand‑crafted component
library that re-creates the look and feel of [shadcn/ui](https://ui.shadcn.com/)
natively in XAML.

> shadcn/ui is a React/Tailwind component collection and cannot run inside WPF
> directly. Instead, its design language — the *zinc* dark palette, soft radii,
> subtle borders, pill buttons, badges, cards and focus rings — has been rebuilt
> as reusable WPF `Style`s and controls so the app looks and behaves like shadcn
> while remaining 100% native XAML.

---

## Requirements

- **Windows 10/11**
- **.NET 8 SDK** (`net8.0-windows`)
- Visual Studio 2022 (17.8+) **or** `dotnet` CLI

> WPF is Windows‑only. The project was authored in a Linux sandbox, so it must be
> built/run on Windows.

## Build & Run

```powershell
cd NauraLauncher
dotnet restore
dotnet run
```

Or open `NauraLauncher.sln` in Visual Studio and press **F5**.

---

## Project structure

```
NauraLauncher/
├─ NauraLauncher.sln
├─ NauraLauncher.csproj          # net8.0-windows, UseWPF
├─ App.xaml / App.xaml.cs        # merges theme dictionaries
├─ MainWindow.xaml(.cs)          # the full APEX shell + custom chrome
│
├─ Themes/                       # design system (shadcn-inspired)
│  ├─ Colors.xaml                # design tokens: palette, brushes, radii
│  ├─ Typography.xaml            # display / body / label text styles
│  ├─ Icons.xaml                 # Lucide-style icon geometries
│  └─ Controls.xaml              # the component library (see below)
│
├─ Controls/
│  └─ IconControl.cs             # lightweight stroked/filled icon renderer
│
├─ Common/                       # MVVM plumbing + converters
│  ├─ ObservableObject.cs
│  ├─ RelayCommand.cs
│  ├─ Converters.cs              # string→visibility, bool→visibility
│  ├─ EqualityConverter.cs       # nav selection highlight
│  └─ IconKeyConverter.cs        # resolve icon by resource key
│
├─ Models/                       # GameEntry, SystemStatus, CategoryTab
├─ ViewModels/                   # MainViewModel, FeatureViewModel
└─ Assets/                       # generated key art + card thumbnails
```

---

## shadcn/ui components re-created in XAML

Defined in `Themes/Controls.xaml`, styled from the tokens in `Themes/Colors.xaml`:

| shadcn component | WPF style key(s) | Where it’s used |
|------------------|------------------|-----------------|
| **Button** (default) | `Button.Primary` | Pre‑Order Pass, active category chip |
| **Button** (secondary) | `Button.Secondary` | muted actions |
| **Button** (outline) | `Button.Outline` | category filter chips |
| **Button** (ghost) | `Button.Ghost` | top nav links, Sort, Details |
| **Button** (icon) | `Button.Icon` | bookmark, filters, window controls |
| **Card** | `Card` | hero spotlight, archive cards, status tiles |
| **Badge** / **Badge (accent)** | `Badge`, `Badge.Accent` | ratings, discounts, ribbons, LIVE DROP |
| **Input** | `Input` + header search field | Search gallery |
| **Separator** | `Separator.V` | status‑bar dividers |
| **ScrollBar** | thin custom template | scrollable regions |
| **Focus ring / radii tokens** | `RadiusSm…Full`, `RingBrush` | global |

Everything is **data‑bound**: nav items, category chips, the curated‑archive grid,
and the status tiles all render from `MainViewModel`, so real data can be dropped
in later with no XAML changes.

## Design fidelity notes

- Frameless window with **custom chrome** (drag, minimize, maximize, close).
- Centered pill navigation with a selected‑state highlight.
- Hero “Premiere Spotlight” card with layered gradient scrims over key art.
- Emerald accent (`#34D399`) for LIVE / POSITIVE / discount states, matching the
  reference.
- Bottom system status bar (SYSTEM READY, patch progress, comm/audio/cloud).
- Artwork in `Assets/` is AI‑generated placeholder key art in the design’s mood;
  swap the files (same names) to use production art.
```
```

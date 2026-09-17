# NauraLauncher Client

Production APEX launcher client for Windows — WPF / .NET 8, Clean Architecture.

## Architecture

- `Core/Entities/`: Domain models for user profiles, catalog, manifests, auction lots, and configuration.
- `Core/Interfaces/`: Strongly-typed contracts for authentication, networking, download/patching, process supervision, and real-time WebSockets.
- `Infrastructure/`: Hardened implementations with Windows DPAPI encryption, TLS 1.3 HTTP client, WSS client, and MySQL connection layer.
- `ViewModels/`: MVVM presentation layer bound to production services via `ServiceContainer`.
- `Views/`: Single frameless page shell with rounded chrome and persistent status bar.

## Build

```bash
dotnet restore NauraLauncher.csproj
dotnet build   NauraLauncher.csproj
dotnet run     --project NauraLauncher.csproj
```

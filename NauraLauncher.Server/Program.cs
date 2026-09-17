using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NauraLauncher.Server.Api;
using NauraLauncher.Server.Database;
using NauraLauncher.Server.Security;
using NauraLauncher.Server.WebSocket;

namespace NauraLauncher.Server;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("====================================================");
        Console.WriteLine("  APEX / NauraLauncher C# .NET 8 Realtime Server    ");
        Console.WriteLine("====================================================");

        var db = new MySqlDatabase();
        await db.InitializeAsync();
        Console.WriteLine("[Database] C# MySQL persistence layer initialized.");

        var jwt = new JwtTokenService();
        var wsServer = new RealtimeWebSocketServer(db);
        var apiHandler = new HttpApiHandler(db, jwt, wsServer);

        int httpPort = int.TryParse(Environment.GetEnvironmentVariable("PORT"), out int p) ? p : 8080;
        int tlsPort = int.TryParse(Environment.GetEnvironmentVariable("TLS_PORT"), out int tp) ? tp : 8443;

        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://*:{httpPort}/");
        try
        {
            listener.Start();
            Console.WriteLine($"[C# Server] Listening on http://0.0.0.0:{httpPort}/ (WebSocket: /ws)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[C# Server] Prefix registration note: {ex.Message}");
            listener.Prefixes.Clear();
            listener.Prefixes.Add($"http://localhost:{httpPort}/");
            listener.Start();
            Console.WriteLine($"[C# Server] Listening on http://localhost:{httpPort}/ (WebSocket: /ws)");
        }

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        _ = Task.Run(async () =>
        {
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    var ctx = await listener.GetContextAsync();
                    if (ctx.Request.IsWebSocketRequest && (ctx.Request.Url?.AbsolutePath == "/ws" || ctx.Request.Url?.AbsolutePath.StartsWith("/ws?") == true))
                    {
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                var wsContext = await ctx.AcceptWebSocketAsync(subProtocol: null);
                                await wsServer.HandleWebSocketContextAsync(wsContext, cts.Token);
                            }
                            catch { }
                        });
                    }
                    else
                    {
                        _ = Task.Run(() => apiHandler.HandleRequestAsync(ctx));
                    }
                }
                catch (HttpListenerException) when (cts.IsCancellationRequested) { break; }
                catch (Exception ex)
                {
                    if (cts.IsCancellationRequested) break;
                    Console.WriteLine($"[C# Server] Context error: {ex.Message}");
                }
            }
        });

        Console.WriteLine("[C# Server] Real-time engine online. Press Ctrl+C to terminate.");
        while (!cts.IsCancellationRequested)
        {
            await Task.Delay(500);
        }

        listener.Stop();
        Console.WriteLine("[C# Server] Terminated cleanly.");
    }
}

using System;
using System.Threading.Tasks;

namespace NauraLauncher.Core.Interfaces;

public interface IGameProcessLauncher
{
    bool IsRunning(string gameTitle);
    int? GetProcessId(string gameTitle);

    Task<bool> LaunchAsync(string gameTitle, string executableName);
    Task TerminateAsync(string gameTitle);

    event EventHandler<(string GameTitle, bool IsRunning, int? Pid)>? ProcessStateChanged;
}

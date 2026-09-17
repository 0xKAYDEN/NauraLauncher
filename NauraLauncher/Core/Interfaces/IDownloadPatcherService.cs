using System;
using System.Threading.Tasks;
using NauraLauncher.Core.Entities;

namespace NauraLauncher.Core.Interfaces;

public interface IDownloadPatcherService
{
    bool IsDownloading { get; }
    DownloadProgressInfo? CurrentProgress { get; }

    Task<bool> StartDownloadOrPatchAsync(string gameTitle, string downloadUrl, string destinationFolder);
    void PauseDownload(string gameTitle);
    void ResumeDownload(string gameTitle);
    void CancelDownload(string gameTitle);

    event EventHandler<DownloadProgressInfo>? ProgressChanged;
}

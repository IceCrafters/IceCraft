namespace IceCraft.Core.Network;

using System;
using Downloader;
using IceCraft.Core.Platform;

public class DownloadManager : IDownloadManager
{
    private readonly HttpClient _httpClient;
    private readonly IFrontendApp _frontendApp;
    private readonly DownloadConfiguration _downloadConfig;

    public DownloadManager(IFrontendApp frontendApp)
    {
        _httpClient = frontendApp.GetClient();
        _frontendApp = frontendApp;

        _downloadConfig = new DownloadConfiguration()
        {
            MaxTryAgainOnFailover = 5,
            MaximumMemoryBufferBytes = 1024 * 1024 * 50,
            ReserveStorageSpaceBeforeStartingDownload = true,
            RequestConfiguration =
            {
                // your custom user agent or your_app_name/app_version.
                UserAgent = $"{_frontendApp.ProductName}/${_frontendApp.ProductVersion}",
            }
        };
    }

    public async Task Download(Uri from, string toFile, INetworkDownloadTask? task = null)
    {
        var downloader = new DownloadService(_downloadConfig);
        downloader.DownloadProgressChanged += (sender, args) =>
        {
            task?.SetDefinitePrecentage(args.ProgressPercentage);
            task?.UpdateSpeed(args.BytesPerSecondSpeed, args.TotalBytesToReceive, args.ReceivedBytesSize);
        };

        downloader.DownloadFileCompleted += (sender, args) =>
        {
            task?.Complete();
        };

        await downloader.DownloadFileTaskAsync(from.ToString(), 
            toFile, 
            _frontendApp.GetCancellationToken());

        if (downloader.IsCancelled)
        {
            throw new TaskCanceledException();
        }
    }
}

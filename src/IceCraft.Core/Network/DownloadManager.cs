namespace IceCraft.Core.Network;

using System;
using System.IO;
using Downloader;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Checksums;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Platform;
using IceCraft.Core.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class DownloadManager : IDownloadManager
{
    private readonly IFrontendApp _frontendApp;
    private readonly DownloadConfiguration _downloadConfig;
    private readonly ILogger<DownloadManager> _logger;
    private readonly IMirrorSearcher _mirrorSearcher;
    private readonly IChecksumRunner _checksumRunner;

    public DownloadManager(IFrontendApp frontendApp, 
        ILogger<DownloadManager> logger, 
        IMirrorSearcher mirrorSearcher,
        IChecksumRunner checksumRunner)
    {
        _frontendApp = frontendApp;
        _logger = logger;
        _mirrorSearcher = mirrorSearcher;

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

        _checksumRunner = checksumRunner;
    }

    public async Task DownloadAsync(Uri from, string toFile, INetworkDownloadTask? task = null)
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

    public async Task DownloadAsync(Uri from, Stream toStream, INetworkDownloadTask? task = null)
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

        using var stream = await downloader.DownloadFileTaskAsync(from.ToString(), 
            _frontendApp.GetCancellationToken());
        await stream.CopyToAsync(toStream);

        if (downloader.IsCancelled)
        {
            throw new TaskCanceledException();
        }
    }

        private static FileStream CreateTemporaryPackageFile(out string path)
    {
        var temporaryName = Path.GetRandomFileName();
        path = Path.Combine(Path.GetTempPath(), temporaryName);
        return File.Create(path);
    }

    public async Task<string> DownloadTemporaryArtefactAsync(CachedPackageInfo packageInfo, INetworkDownloadTask? downloadTask = null)
    {
         // Get the best mirror.
        _logger.LogInformation("Probing mirrors");
        var bestMirror = await _mirrorSearcher.GetBestMirrorAsync(packageInfo.Mirrors)
            ?? throw new InvalidOperationException("No best mirror can be found.");

        return await DownloadTemporaryArtefactAsync(bestMirror, downloadTask);
    }

    public async Task<string> DownloadTemporaryArtefactAsync(ArtefactMirrorInfo mirror, INetworkDownloadTask? downloadTask = null)
    {
        await using var tempFile = CreateTemporaryPackageFile(out var path);
            await DownloadAsync(mirror, tempFile, downloadTask);
            return path;
    }

    public async Task<string> DownloadTemporaryArtefactSecureAsync(CachedPackageInfo packageInfo, INetworkDownloadTask? downloadTask = null)
    {
        // Get the best mirror.
        _logger.LogInformation("Probing mirrors");
        var bestMirror = await _mirrorSearcher.GetBestMirrorAsync(packageInfo.Mirrors)
                         ?? throw new InvalidOperationException("No best mirror can be found.");

        return await DownloadTemporaryArtefactSecureAsync(bestMirror, downloadTask);
    }

    public async Task<string> DownloadTemporaryArtefactSecureAsync(ArtefactMirrorInfo mirror, INetworkDownloadTask? downloadTask = null)
    {
        var tempStream = CreateTemporaryPackageFile(out var path);
        await using (var tempFile = tempStream)
        {
            await DownloadAsync(mirror, tempFile, downloadTask);
        }

        // ReSharper disable once InvertIf
        if (!await _checksumRunner.ValidateLocal(mirror, path))
        {
            _logger.LogTrace("Remote hash: {Checksum}", mirror.Checksum);
            throw new KnownException("Artefact hash mismatches downloaded file.");
        }

        return path;
    }

    public async Task DownloadAsync(ArtefactMirrorInfo bestMirror, Stream stream,
        INetworkDownloadTask? downloadTask = null)
    {
        await DownloadAsync(bestMirror.DownloadUri,
            stream,
            downloadTask);
    }
}

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

    #region Display Helpers

    private const double Megabyte = 1024 * 1024;
    private const double Kilobyte = 1024;

    private static string GetUserReadableSize(double size)
    {
        if (size >= Megabyte)
        {
            return $"{size / Megabyte:N3} MiB";
        }

        if (size >= Kilobyte)
        {
            return $"{size / Kilobyte:N3} KiB";
        }

        return $"{size:N3} B";
    }

    private static string GetUserReadableSpeed(double bps)
    {
        if (bps >= Megabyte)
        {
            return $"{bps / Megabyte:N3} MiB/s";
        }

        if (bps >= Kilobyte)
        {
            return $"{bps / Kilobyte:N3} KiB/s";
        }

        return $"{bps:N3} B/s";
    }

    private static void UpdateSpeed(IProgressedTask? task, string? fileName, double bps, long totalBytesToReceive, long receivedBytes)
    {
        if (task == null)
        {
            return;
        }

        task.SetText(fileName == null
        ? $"{GetUserReadableSize(receivedBytes)}/{GetUserReadableSize(totalBytesToReceive)} - {GetUserReadableSpeed(bps)}"
        : $"{fileName} | {GetUserReadableSize(receivedBytes)}/{GetUserReadableSize(totalBytesToReceive)} - {GetUserReadableSpeed(bps)}");
        
    }

    #endregion

    public async Task DownloadAsync(Uri from, string toFile, IProgressedTask? task = null, string? fileName = null)
    {
        var downloader = new DownloadService(_downloadConfig);

        downloader.DownloadProgressChanged += (sender, args) =>
        {
            task?.SetDefinitePrecentage(args.ProgressPercentage);
            UpdateSpeed(task, fileName, args.BytesPerSecondSpeed, args.TotalBytesToReceive, args.ReceivedBytesSize);
        };

        await downloader.DownloadFileTaskAsync(from.ToString(),
            toFile,
            _frontendApp.GetCancellationToken());

        if (downloader.IsCancelled)
        {
            throw new TaskCanceledException();
        }
    }

    public async Task DownloadAsync(Uri from, Stream toStream, IProgressedTask? task = null, string? fileName = null)
    {
        var downloader = new DownloadService(_downloadConfig);
        downloader.DownloadProgressChanged += (sender, args) =>
        {
            task?.SetDefinitePrecentage(args.ProgressPercentage);
            UpdateSpeed(task, fileName, args.BytesPerSecondSpeed, args.TotalBytesToReceive, args.ReceivedBytesSize);
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

    public async Task<string> DownloadTemporaryArtefactAsync(CachedPackageInfo packageInfo, IProgressedTask? downloadTask = null, string? fileName = null)
    {
        // Get the best mirror.
        _logger.LogInformation("Probing mirrors");
        var bestMirror = await _mirrorSearcher.GetBestMirrorAsync(packageInfo.Mirrors)
            ?? throw new InvalidOperationException("No best mirror can be found.");

        return await DownloadTemporaryArtefactAsync(bestMirror, downloadTask, fileName);
    }

    public async Task<string> DownloadTemporaryArtefactAsync(ArtefactMirrorInfo mirror, IProgressedTask? downloadTask = null, string? fileName = null)
    {
        await using var tempFile = CreateTemporaryPackageFile(out var path);
        await DownloadAsync(mirror, tempFile, downloadTask, fileName);
        return path;
    }

    public async Task<string> DownloadTemporaryArtefactSecureAsync(CachedPackageInfo packageInfo, IProgressedTask? downloadTask = null, string? fileName = null)
    {
        // Get the best mirror.
        _logger.LogInformation("Probing mirrors");
        var bestMirror = await _mirrorSearcher.GetBestMirrorAsync(packageInfo.Mirrors)
                         ?? throw new InvalidOperationException("No best mirror can be found.");

        return await DownloadTemporaryArtefactSecureAsync(packageInfo.Artefact, bestMirror, downloadTask, fileName);
    }

    public async Task<string> DownloadTemporaryArtefactSecureAsync(RemoteArtefact artefact,
        ArtefactMirrorInfo mirror,
        IProgressedTask? downloadTask = null,
        string? fileName = null)
    {
        var tempStream = CreateTemporaryPackageFile(out var path);
        await using (var tempFile = tempStream)
        {
            await DownloadAsync(mirror, tempFile, downloadTask, fileName);
        }

        // ReSharper disable once InvertIf
        if (!await _checksumRunner.ValidateLocal(artefact, path))
        {
            _logger.LogTrace("Remote hash: {Checksum}", artefact.Checksum);
            throw new KnownException("Artefact hash mismatches downloaded file.");
        }

        return path;
    }

    public async Task DownloadAsync(ArtefactMirrorInfo bestMirror, 
        Stream stream,
        IProgressedTask? downloadTask = null,
        string? fileName = null)
    {
        await DownloadAsync(bestMirror.DownloadUri,
            stream,
            downloadTask,
            fileName);
    }
}

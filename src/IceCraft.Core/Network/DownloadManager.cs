// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Network;

using System;
using System.IO;
using Downloader;
using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Archive.Indexing;
using IceCraft.Api.Client;
using IceCraft.Api.Exceptions;
using IceCraft.Api.Network;

public class DownloadManager : IDownloadManager
{
    private readonly IFrontendApp _frontendApp;
    private readonly DownloadConfiguration _downloadConfig;
    private readonly IMirrorSearcher _mirrorSearcher;
    private readonly IChecksumRunner _checksumRunner;

    public DownloadManager(IFrontendApp frontendApp,
        IMirrorSearcher mirrorSearcher,
        IChecksumRunner checksumRunner)
    {
        _frontendApp = frontendApp;
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
            return $"{bps / Megabyte:N2} MiB/s";
        }

        if (bps >= Kilobyte)
        {
            return $"{bps / Kilobyte:N2} KiB/s";
        }

        return $"{bps:N2} B/s";
    }

    private static void UpdateSpeed(IProgressedTask? task, string? fileName, double bps, long totalBytesToReceive, long receivedBytes)
    {
        if (task == null)
        {
            return;
        }

        task.SetText(fileName == null
        ? $"{GetUserReadableSize(receivedBytes)}/{GetUserReadableSize(totalBytesToReceive)} - {GetUserReadableSpeed(bps)}"
        : $"{fileName} | {GetUserReadableSpeed(bps)}");

    }

    #endregion


    public async Task<DownloadResult> DownloadAsync(Uri from, string toFile, IProgressedTask? task = null, string? fileName = null)
    {
        var downloader = new DownloadService(_downloadConfig);

        downloader.DownloadProgressChanged += (_, args) =>
        {
            if (args.TotalBytesToReceive <= 0)
            {
                task?.SetIntermediateProgress();
            }
            else
            {
                task?.SetDefinitePrecentage(args.ProgressPercentage);
            }

            UpdateSpeed(task, fileName, args.BytesPerSecondSpeed, args.TotalBytesToReceive, args.ReceivedBytesSize);
        };

        await downloader.DownloadFileTaskAsync(from.ToString(),
            toFile,
            _frontendApp.GetCancellationToken());

        if (downloader.IsCancelled)
        {
            throw new TaskCanceledException();
        }

        if (downloader.Status == DownloadStatus.Failed)
        {
            return DownloadResult.Failed;
        }

        return DownloadResult.Succeeded;
    }

    public async Task<DownloadResult> DownloadAsync(Uri from, Stream toStream, IProgressedTask? task = null, string? fileName = null)
    {
        var downloader = new DownloadService(_downloadConfig);
        downloader.DownloadProgressChanged += (_, args) =>
        {
            task?.SetDefinitePrecentage(args.ProgressPercentage);
            UpdateSpeed(task, fileName, args.BytesPerSecondSpeed, args.TotalBytesToReceive, args.ReceivedBytesSize);
        };

        await using var stream = await downloader.DownloadFileTaskAsync(from.ToString(),
            _frontendApp.GetCancellationToken());
        if (stream == null)
        {
            throw new KnownException("Failed to initiate download");
        }
        await stream.CopyToAsync(toStream);
        await toStream.FlushAsync();

        if (downloader.IsCancelled)
        {
            if (downloader.Status == DownloadStatus.Failed)
            {
                return DownloadResult.Failed;
            }

            throw new KnownException($"Download cancelled: status {downloader.Status}");
        }

        return DownloadResult.Succeeded;
    }

    private static FileStream CreateTemporaryPackageFile(out string path)
    {
        var temporaryName = Path.GetRandomFileName();
        path = Path.Combine(Path.GetTempPath(), temporaryName);
        return File.Create(path);
    }

    [Obsolete("Use DownloadManagerExtensions.DownloadAtrefactAsync instead.")]
    public async Task<string> DownloadTemporaryArtefactAsync(CachedPackageInfo packageInfo, IProgressedTask? downloadTask = null, string? fileName = null)
    {
        // Get the best mirror.
        _frontendApp.Output.Verbose("Searching best mirror for {0} ({1})...",
            packageInfo.Metadata.Id,
            packageInfo.Metadata.Version);

        var bestMirror = await _mirrorSearcher.GetBestMirrorAsync(packageInfo.Mirrors)
            ?? throw new InvalidOperationException("No best mirror can be found.");

        return await DownloadTemporaryArtefactAsync(bestMirror, downloadTask, fileName);
    }

    [Obsolete("Callers should manage temporary files themselves.")]
    public async Task<string> DownloadTemporaryArtefactAsync(ArtefactMirrorInfo mirror, IProgressedTask? downloadTask = null, string? fileName = null)
    {
        await using var tempFile = CreateTemporaryPackageFile(out var path);
        await DownloadAsync(mirror, tempFile, downloadTask, fileName);
        return path;
    }

    [Obsolete("Callers should manage temporary files themselves.")]
    public async Task<string> DownloadTemporaryArtefactSecureAsync(CachedPackageInfo packageInfo, IProgressedTask? downloadTask = null, string? fileName = null)
    {
        // Get the best mirror.
        _frontendApp.Output.Verbose("Searching best mirror for {0} ({1})...",
            packageInfo.Metadata.Id,
            packageInfo.Metadata.Version);

        var bestMirror = await _mirrorSearcher.GetBestMirrorAsync(packageInfo.Mirrors)
                         ?? throw new InvalidOperationException("No best mirror can be found.");

        return await DownloadTemporaryArtefactSecureAsync(packageInfo.Artefact, bestMirror, downloadTask, fileName);
    }

    [Obsolete("Use DownloadTemporaryArtefactSecureAsync(IArtefactDefinition, ...) instead.")]
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
            throw new KnownException("Artefact hash mismatches downloaded file.");
        }

        return path;
    }

    [Obsolete("Use DownloadManagerExtensions.DownloadAtrefactAsync instead.")]
    public async Task<DownloadResult> DownloadAsync(CachedPackageInfo packageInfo, Stream to, IProgressedTask? downloadTask = null, string? fileName = null)
    {
        // Get the best mirror.
        _frontendApp.Output.Verbose("Searching best mirror for {0} ({1})...",
            packageInfo.Metadata.Id,
            packageInfo.Metadata.Version);

        var bestMirror = await _mirrorSearcher.GetBestMirrorAsync(packageInfo.Mirrors)
                         ?? throw new InvalidOperationException("No best mirror can be found.");

        return await DownloadAsync(bestMirror, to, downloadTask, fileName);
    }

    [Obsolete("Use DownloadManagerExtensions.DownloadAtrefactAsync instead.")]
    public async Task<DownloadResult> DownloadAsync(ArtefactMirrorInfo bestMirror,
        Stream stream,
        IProgressedTask? downloadTask = null,
        string? fileName = null)
    {
        return await DownloadAsync(bestMirror.DownloadUri,
            stream,
            downloadTask,
            fileName);
    }

    [Obsolete("Callers should manage temporary files and compare checksums themselves.")]
    public async Task<string> DownloadTemporaryArtefactSecureAsync(IArtefactDefinition artefact,
        ArtefactMirrorInfo mirror,
        IProgressedTask? downloadTask = null, string? fileName = null)
    {
        var tempStream = CreateTemporaryPackageFile(out var path);
        await using (var tempFile = tempStream)
        {
            await DownloadAsync(mirror, tempFile, downloadTask, fileName);
        }

        // ReSharper disable once InvertIf
        if (!await _checksumRunner.ValidateAsync(artefact, path))
        {
            throw new KnownException("Artefact hash mismatches downloaded file.");
        }

        return path;
    }
}

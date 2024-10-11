// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Network;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Archive.Indexing;
using IceCraft.Api.Client;
using IceCraft.Api.Network;

public class MissionDownloadManager : IDownloadManager
{
    private readonly IFrontendApp _frontend;

    public MissionDownloadManager(IFrontendApp frontend)
    {
        _frontend = frontend;
    }

    public async Task<DownloadResult> DownloadAsync(Uri from, string toFile, IProgressedTask? downloadTask = null, string? fileName = null)
    {
        var target = new DownloadTarget(from, fileName: fileName);
        var mission = new DownloadMission(_frontend.GetClient());

        if (downloadTask != null)
        {
            AttachTask(downloadTask, mission);
        }

        var result = await mission.DownloadAsync(target, toFile, _frontend.GetCancellationToken());
        return GetDownloadResult(result);
    }

    public async Task<DownloadResult> DownloadAsync(Uri from,
        Stream toStream,
        IProgressedTask? downloadTask = null,
        string? fileName = null)
    {
        var target = new DownloadTarget(from, -1, fileName: fileName);
        var mission = new DownloadMission(_frontend.GetClient());

        if (downloadTask != null)
        {
            AttachTask(downloadTask, mission);
        }

        var result = await mission.DownloadAsync(target, toStream, _frontend.GetCancellationToken());
        return GetDownloadResult(result);
    }

    [Obsolete("Use DownloadManagerExtensions.DownloadAtrefactAsync instead.")]
    public Task<DownloadResult> DownloadAsync(ArtefactMirrorInfo fromMirror, Stream toStream, IProgressedTask? downloadTask = null, string? fileName = null)
    {
        throw new NotSupportedException();
    }

    [Obsolete("Use DownloadManagerExtensions.DownloadAtrefactAsync instead.")]
    [DoesNotReturn]
    public Task<DownloadResult> DownloadAsync(CachedPackageInfo packageInfo, Stream to, IProgressedTask? downloadTask = null, string? fileName = null)
    {
        throw new NotSupportedException();
    }

    [Obsolete("Callers should manage temporary files themselves.")]
    public Task<string> DownloadTemporaryArtefactAsync(CachedPackageInfo packageInfo, IProgressedTask? downloadTask = null, string? fileName = null)
    {
        throw new NotSupportedException();
    }

    [Obsolete("Callers should manage temporary files themselves.")]
    public Task<string> DownloadTemporaryArtefactAsync(ArtefactMirrorInfo mirror, IProgressedTask? downloadTask = null, string? fileName = null)
    {
        throw new NotSupportedException();
    }

    [Obsolete("Callers should manage temporary files themselves.")]
    public Task<string> DownloadTemporaryArtefactSecureAsync(CachedPackageInfo packageInfo, IProgressedTask? downloadTask = null, string? fileName = null)
    {
        throw new NotSupportedException();
    }

    [Obsolete("Callers should manage temporary files themselves.")]
    public Task<string> DownloadTemporaryArtefactSecureAsync(IArtefactDefinition artefact, ArtefactMirrorInfo mirror, IProgressedTask? downloadTask = null, string? fileName = null)
    {
        throw new NotSupportedException();
    }

    #region Download Helpers

    private DownloadResult GetDownloadResult(DownloadMissionResult result)
    {
        if (result.Result == DownloadResult.Failed)
        {
            _frontend.Output.Error(result.Exception?.Message ?? "Unknown download error");
            _frontend.Output.Verbose(result.Exception?.ToString() ?? "No details");
        }

        return result.Result;
    }

    private static void AttachTask(IProgressedTask downloadTask, DownloadMission mission)
    {
        mission.ProgressCallback = (_, progress) =>
        {
            if (progress.IsIndeterminate)
            {
                downloadTask.SetIntermediateProgress();
            }
            else
            {
                downloadTask.SetDefinitePrecentage(progress.Progress);
            }
        };
    }

    #endregion
}

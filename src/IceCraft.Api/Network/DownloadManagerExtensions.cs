// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Network;

using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Archive.Indexing;
using IceCraft.Api.Client;

public static class DownloadManagerExtensions
{
    public static async Task<DownloadResult> DownloadArtefactAsync(this IDownloadManager downloadManager,
        IMirrorSearcher mirrorSearcher,
        CachedPackageInfo packageInfo,
        Stream toStream,
        IProgressedTask? downloadTask = null,
        string? fileName = null)
    {
        var bestMirror = await mirrorSearcher.GetBestMirrorAsync(packageInfo.Mirrors)
                        ?? throw new InvalidOperationException("No best mirror can be found.");

        return await downloadManager.DownloadArtefactAsync(bestMirror, toStream, downloadTask, fileName);
    }

    public static async Task<DownloadResult> DownloadArtefactAsync(this IDownloadManager downloadManager,
        ArtefactMirrorInfo mirrorInfo,
        Stream toStream,
        IProgressedTask? downloadTask = null,
        string? fileName = null)
    {
        return await downloadManager.DownloadAsync(mirrorInfo.DownloadUri,
            toStream,
            downloadTask,
            fileName);
    }
}

// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Network;

using System.Diagnostics.CodeAnalysis;

public readonly record struct DownloadMissionProgress
{
    [SetsRequiredMembers]
    public DownloadMissionProgress(int progress, bool indeterminate, int bytesDownloaded)
    {
        Progress = progress;
        IsIndeterminate = indeterminate;
        BytesDownloaded = bytesDownloaded;
    }

    public required int Progress { get; init; }
    public required bool IsIndeterminate { get; init; }
    public required int BytesDownloaded { get; init; }

    public static DownloadMissionProgress Indeterminate(int bytesDownloaded)
    {
        return new DownloadMissionProgress(-1, true, bytesDownloaded);
    }

    public static DownloadMissionProgress Determinate(int progress, int bytesDownloaded)
    {
        return new DownloadMissionProgress(progress, false, bytesDownloaded);
    }
}

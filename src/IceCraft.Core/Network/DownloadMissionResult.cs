// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Network;

using System.Net;
using IceCraft.Api.Network;

public readonly record struct DownloadMissionResult
{
    public required DownloadResult Result { get; init; }
    public Exception? Exception { get; init; }

    public static DownloadMissionResult CreateSuccess()
    {
        return new DownloadMissionResult
        {
            Result = DownloadResult.Succeeded
        };
    }

    public static DownloadMissionResult CreateWithException(Exception exception)
    {
        return new DownloadMissionResult
        {
            Result = DownloadResult.Failed,
            Exception = exception
        };
    }

    public static DownloadMissionResult CreateWithStatusCode(HttpStatusCode statusCode)
    {
        return CreateWithException(DownloadException.CreateWithStatusCode(statusCode));
    }
}

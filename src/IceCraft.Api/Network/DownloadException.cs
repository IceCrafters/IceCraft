// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Network;

using System.Net;

public class DownloadException : Exception
{
    public DownloadException()
    {
    }

    public DownloadException(string? message) : base(message)
    {
    }

    public DownloadException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public static DownloadException CreateWithStatusCode(HttpStatusCode statusCode)
    {
        return new DownloadException($"Remote returned status code {statusCode} ({(int)statusCode})");
    }
}

// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Network;

using System.Diagnostics.CodeAnalysis;

public readonly record struct DownloadTarget
{
    [SetsRequiredMembers]
    public DownloadTarget(Uri link, int contentLength = -1)
    {
        Link = link;
        ContentLength = contentLength;
    }

    /// <summary>
    /// Gets the URI to the remote where the file will be downloaded from.
    /// </summary>
    public required Uri Link { get; init; }

    /// <summary>
    /// Gets the predetermined content length for the objective. The <c>Content-Length</c> header returned
    /// by server however will override this property.
    /// </summary>
    public int ContentLength { get; init; }
}

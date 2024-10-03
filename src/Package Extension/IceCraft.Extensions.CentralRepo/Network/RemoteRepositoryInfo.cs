// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

using System.Diagnostics.CodeAnalysis;

namespace IceCraft.Extensions.CentralRepo.Network;

public readonly struct RemoteRepositoryInfo
{
    [SetsRequiredMembers]
    public RemoteRepositoryInfo(Uri uri, string? subfolder = null)
    {
        this.Uri = uri;
        this.Subfolder = subfolder;
    }

    public required Uri Uri { get; init; }
    public string? Subfolder { get; init; }
}

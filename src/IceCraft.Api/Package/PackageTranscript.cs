// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Package;

public sealed record PackageTranscript
{
    public required IReadOnlyList<PackageAuthorInfo> Authors { get; init; }

    public string? Description { get; init; }

    public string? License { get; init; }

    public PackageAuthorInfo Maintainer { get; init; }

    public PackageAuthorInfo PluginMaintainer { get; init; }
}

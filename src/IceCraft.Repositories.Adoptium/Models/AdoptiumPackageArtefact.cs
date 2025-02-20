﻿// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Repositories.Adoptium.Models;

using JetBrains.Annotations;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers,
    Reason = "JSON model")]
internal class AdoptiumPackageArtefact
{
    public required string Name { get; init; }
    public required Uri Link { get; init; }
    public long Size { get; init; }
    public string? Checksum { get; init; }
    public Uri? ChecksumLink { get; init; }
    public Uri? SignatureLink { get; init; }
    public long DownloadCount { get; init; }
    public Uri? MetadataLink { get; init; }
}

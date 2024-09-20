// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Repositories.Adoptium.Models;

using JetBrains.Annotations;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers,
    Reason = "JSON model")]
internal class AdoptiumBinaryRelease
{
    public IList<AdoptiumBinaryAsset>? Binaries { get; init; }
    public required string ReleaseName { get; init; }
    public required Uri ReleaseLink { get; init; }
    public string? Vendor { get; init; }
    public DateTime? Timestamp { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public AdoptiumReleaseVersion? VersionData { get; init; }
}

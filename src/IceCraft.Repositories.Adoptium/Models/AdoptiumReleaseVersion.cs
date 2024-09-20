// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Repositories.Adoptium.Models;

using JetBrains.Annotations;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers,
    Reason = "JSON model")]
public class AdoptiumReleaseVersion
{
    public int Major { get; init; }
    public int Minor { get; init; }
    public int Security { get; init; }
    public int? Patch { get; init; }
    public string? Pre { get; init; }
    public int? AdoptBuildNumber { get; init; }
    public required string Semver { get; init; }
    public required string OpenjdkVersion { get; init; }
    public int Build { get; init; }
    public string? Optional { get; init; }
}

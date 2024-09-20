// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation.Dependency;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using IceCraft.Api.Serialization;
using Semver;

public readonly record struct DependencyReference
{
    public DependencyReference()
    {
    }

    [SetsRequiredMembers]
    public DependencyReference(string packageId, SemVersionRange versionRange)
    {
        PackageId = packageId;
        VersionRange = versionRange;
    }
    
    public required string PackageId { get; init; }
    
    [JsonConverter(typeof(SemVersionRangeConverter))]
    public required SemVersionRange VersionRange { get; init; }
}
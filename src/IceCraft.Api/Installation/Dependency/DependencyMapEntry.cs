// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation.Dependency;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using IceCraft.Api.Serialization;
using Semver;

public sealed record DependencyMapEntry
{
    public DependencyMapEntry()
    {
    }

    [SetsRequiredMembers]
    public DependencyMapEntry(string packageName, SemVersion version)
    {
        PackageName = packageName;
        Version = version;

        Dependencies = [];
        Dependents = [];
    }
    
    public required string PackageName { get; init; }
    [JsonConverter(typeof(SemVersionConverter))]
    public required SemVersion Version { get; init; }

    public bool HasUnsatisifiedDependencies { get; set; }

    public required IList<PackageReference> Dependencies { get; init; }

    public required IList<PackageReference> Dependents { get; init; }
}
// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Package;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package.Data;
using IceCraft.Api.Serialization;
using Semver;

public sealed record PackageMeta
{
    public PackageMeta()
    {
    }

    [SetsRequiredMembers]
    public PackageMeta(string id, 
        SemVersion version, 
        DateTime releaseDate, 
        PackagePluginInfo pluginInfo,
        DependencyCollection? dependencies = null,
        bool unitary = false,
        PackageTranscript? transcript = null)
    {
        Id = id;
        Version = version;
        ReleaseDate = releaseDate;
        PluginInfo = pluginInfo;
        Dependencies = dependencies;
        Unitary = unitary;
        Transcript = transcript;
    }

    public required string Id { get; init; }

    [JsonConverter(typeof(SemVersionConverter))]
    public required SemVersion Version { get; init; }

    public required DateTime ReleaseDate { get; init; }

    public required PackagePluginInfo PluginInfo { get; init; }
    
    public DependencyCollection? Dependencies { get; init; }

    public DependencyCollection? ConflictsWith { get; init; }

    [Obsolete("Use CustomData instead.")]
    public IDictionary<string, string?>? AdditionalMetadata { get; init; }
    
    /// <summary>
    /// Gets the dictionary containing custom data for the current package.
    /// </summary>
    public PackageCustomDataDictionary? CustomData { get; init; }

    /// <summary>
    /// Gets a value indicating whether this package should install to a fixed location and the previous version must be
    /// uninstalled before installing the new version.
    /// </summary>
    public bool Unitary { get; init; }

    /// <summary>
    /// Gets the human-readable information associated with this package.
    /// </summary>
    public PackageTranscript? Transcript { get; init; }
}

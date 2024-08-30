namespace IceCraft.Core.Archive.Packaging;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using IceCraft.Core.Archive.Dependency;
using IceCraft.Core.Serialization;
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

    public IDictionary<string, string>? AdditionalMetadata { get; init; }

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

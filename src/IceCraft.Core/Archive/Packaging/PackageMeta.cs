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
        DependencyCollection? dependencies = null)
    {
        Id = id;
        Version = version;
        ReleaseDate = releaseDate;
        PluginInfo = pluginInfo;
        Dependencies = dependencies;
    }

    public required string Id { get; init; }

    [JsonConverter(typeof(SemVersionConverter))]
    public required SemVersion Version { get; init; }

    public required DateTime ReleaseDate { get; init; }

    public required PackagePluginInfo PluginInfo { get; init; }
    
    public DependencyCollection? Dependencies { get; init; }

    public IDictionary<string, string>? AdditionalMetadata { get; init; }

    /// <summary>
    /// Gets a value indicating whether this package can have multiple versions installed into
    /// the same IceCraft instance, side by side.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Common examples of packages that cannot exist side by side are packages that contain executables which
    /// must be mapped via <see cref="Installation.Execution.IExecutableManager"/>.
    /// </para>
    /// <para>
    /// It is recommended that only transient package dependencies use this mode.
    /// </para>
    /// </remarks>
    public bool CanMultipleVersionsCoexist { get; init; }
}

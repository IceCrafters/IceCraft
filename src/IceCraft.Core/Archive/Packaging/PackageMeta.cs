namespace IceCraft.Core.Archive.Packaging;

using System.Diagnostics.CodeAnalysis;

public sealed record PackageMeta
{
    public PackageMeta()
    {
    }

    [SetsRequiredMembers]
    public PackageMeta(string id, string version, DateTime releaseDate, PackagePluginInfo pluginInfo)
    {
        Id = id;
        Version = version;
        ReleaseDate = releaseDate;
        PluginInfo = pluginInfo;
    }

    public required string Id { get; init; }

    public required string Version { get; init; }

    public required DateTime ReleaseDate { get; init; }

    public required PackagePluginInfo PluginInfo { get; init; }

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

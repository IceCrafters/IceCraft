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
}

using System;

namespace IceCraft.Core.Archive.Indexing;

public sealed record CachedPackageSeriesInfo
{
    public required string Name { get; init; }
    public required IDictionary<string, CachedPackageInfo> Versions { get; init; }
    public string? LatestVersion { get; init; }
}

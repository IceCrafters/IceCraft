namespace IceCraft.Core.Archive.Indexing;
using System.Text.Json.Serialization;
using IceCraft.Core.Serialization;
using Semver;

public sealed record CachedPackageSeriesInfo
{
    public required string Name { get; init; }
    public required IDictionary<string, CachedPackageInfo> Versions { get; init; }

    [JsonConverter(typeof(SemVersionConverter))]
    public SemVersion? LatestVersion { get; init; }
}

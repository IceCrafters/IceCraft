namespace IceCraft.Core.Archive.Indexing;
using System.Text.Json.Serialization;
using IceCraft.Core.Serialization;
using Semver;

public sealed record CachedPackageSeriesInfo
{
    public required string Name { get; init; }
    public required IDictionary<string, CachedPackageInfo> Versions { get; init; }

    [JsonConverter(typeof(SemVersionConverter))]
    [Obsolete("This property is deprecated and CachedIndexer no longer fills it. Compare semvers instead.")]
    public SemVersion? LatestVersion { get; init; }
}

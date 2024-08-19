namespace IceCraft.Core.Archive.Indexing;

using Semver;

public static class VersioningExtensions
{
    public static SemVersion GetLatestSemVersion(this IDictionary<string, CachedPackageInfo> cache)
    {
        return cache.Keys.Select(x => SemVersion.Parse(x, SemVersionStyles.Strict))
            .OrderByDescending(x => x)
            .First();
    }
}

namespace IceCraft.Api.Archive.Indexing;

using IceCraft.Api.Installation;
using Semver;

public static class VersioningExtensions
{
    public static SemVersion GetLatestSemVersion(this IDictionary<string, CachedPackageInfo> cache,
        bool includePrerelease = false)
    {
        return cache.Keys.Select(x => SemVersion.Parse(x, SemVersionStyles.Strict))
            .Where(x => !x.IsPrerelease || includePrerelease)
            .OrderByDescending(x => x)
            .First();
    }
    
    public static SemVersion GetLatestSemVersion(this PackageInstallationIndex cache,
        bool includePrerelease = false)
    {
        return Enumerable.Select<string, SemVersion>(cache.Keys, x => SemVersion.Parse(x, SemVersionStyles.Strict))
            .Where(x => !x.IsPrerelease || includePrerelease)
            .OrderByDescending(x => x)
            .First();
    }
}

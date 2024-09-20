// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

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
    
    public static SemVersion? GetLatestSemVersionOrDefault(this IDictionary<string, CachedPackageInfo> cache,
        bool includePrerelease = false)
    {
        return cache.Keys.Select(x => SemVersion.Parse(x, SemVersionStyles.Strict))
            .Where(x => !x.IsPrerelease || includePrerelease)
            .OrderByDescending(x => x)
            .FirstOrDefault();
    }
    
    public static SemVersion GetLatestSemVersion(this PackageInstallationIndex cache,
        bool includePrerelease = false)
    {
        return cache.Keys.Select<string, SemVersion>(x => SemVersion.Parse(x, SemVersionStyles.Strict))
            .Where(x => !x.IsPrerelease || includePrerelease)
            .OrderByDescending(x => x)
            .First();
    }
}

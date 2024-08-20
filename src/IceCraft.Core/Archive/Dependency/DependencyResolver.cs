namespace IceCraft.Core.Archive.Dependency;

using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Installation;
using Microsoft.Extensions.Logging;

public class DependencyResolver : IDependencyResolver
{
    private readonly IPackageInstallManager _installManager;
    private readonly ILogger<DependencyResolver> _logger;

    public DependencyResolver(IPackageInstallManager installManager,
        ILogger<DependencyResolver> logger)
    {
        _installManager = installManager;
        _logger = logger;
    }

    private static PackageMeta? SelectLatest(IEnumerable<PackageMeta> packages)
    {
        PackageMeta? best = null;

        foreach (var meta in packages)
        {
            if (meta.Version.CompareSortOrderTo(best?.Version) > 0)
            {
                best = meta;
            }
        }

        return best;
    }

    public async Task ResolveTree(PackageMeta meta, PackageIndex index, ISet<PackageMeta> setToAppend)
    {
        // The dependency tree trunk.
        var depTrunk = new HashSet<PackageMeta>(meta.Dependencies?.Count ?? 10);
        
        // The collection of all dependency tree branches.
        var depBranches = new List<HashSet<PackageMeta>>(meta.Dependencies?.Count ?? 10);

        // Resolve the dependencies at trunk level.
        await ResolveDependencies(meta, index, depTrunk);

        // Step 1: generate dependency trees for all dependencies.
        await Task.Run(async () =>
        {
            foreach (var dependency in depTrunk)
            {
                if (dependency.Dependencies == null || dependency.Dependencies.Count == 0)
                {
                    continue;
                }

                // Create a dependency tree for the current dependency, and resolve dependencies
                // into this dependency tree.
                var depBranch = new HashSet<PackageMeta>(dependency.Dependencies.Count);

                await ResolveTree(dependency, index, depBranch);
                
                // Expand here so we don't do it later in Step 2.
                depTrunk.EnsureCapacity(depTrunk.Count + depBranch.Count);
                depBranches.Add(depBranch);
            }
        });

        // Step 2: merge all dependency trees in to the trunk.
        await Task.Run(() =>
        {
            foreach (var entry in 
                     depBranches.SelectMany(branch => branch))
            {
                depTrunk.Add(entry);
            }
        });
        
        // Faster logic for expandable lists.
        if (setToAppend is HashSet<PackageMeta> expandableList)
        {
            await Task.Run(() =>
            {
                expandableList.EnsureCapacity(expandableList.Count + depTrunk.Count);
                foreach (var entry in depTrunk)
                {
                    setToAppend.Add(entry);
                }
            });
            return;
        }

        // Much slower logic.
        _logger.LogWarning("ResolveTree was called with a set that cannot EnsureCapacity");
        _logger.LogWarning("Expect upcoming worsened performance");
        
        await Task.Run(() =>
        {
            foreach (var entry in depTrunk)
            {
                setToAppend.Add(entry);
            }
        });
    }

    public async Task ResolveDependencies(PackageMeta meta, PackageIndex index, ISet<PackageMeta> listToAppend)
    {
        if (meta.Dependencies == null)
        {
            return;
        }

        if (listToAppend is HashSet<PackageMeta> expandableSet)
        {
            expandableSet.EnsureCapacity(expandableSet.Count + meta.Dependencies.Count);
        }

        foreach (var dependency in meta.Dependencies)
        {
            if (await _installManager.IsInstalledAsync(dependency))
            {
                // No need to install package that is already exists.
                continue;
            }

            if (dependency.PackageId == meta.Id)
            {
                // Fail: Self reference
                throw DependencyException.SelfReference(dependency);
            }

            if (!index.TryGetValue(dependency.PackageId, out var seriesInfo))
            {
                // Fail: No such package series
                throw DependencyException.Unsatisfied(dependency);
            }

            // Selects the latest version from an entire list of package metas that satisfies the condition.
            var depMeta = await Task.Run(() => SelectLatest(
                seriesInfo.Versions.Values
                    .Select(x => x.Metadata)
                    .Where(x => dependency.VersionRange.Contains(x.Version))));

            if (depMeta == null)
            {
                // Fail: No matching version available
                throw DependencyException.Unsatisfied(dependency);
            }

            listToAppend.Add(depMeta);
        }
    }
}
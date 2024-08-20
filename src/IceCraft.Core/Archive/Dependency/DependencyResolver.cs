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
        var resolvingStack = new Stack<PackageMeta>();
        resolvingStack.Push(meta);
        
        // Step 1: generate dependency trees for all dependencies.
        await Task.Run(async () =>
        {
            while (resolvingStack.Count != 0)
            {
                var package = resolvingStack.Pop();
                if (package.Dependencies == null || package.Dependencies.Count == 0)
                {
                    continue;
                }
                
                var deps = ResolveDependencies(package, index);
                
                // Add into dependency trunk and push the dependency package into the resolving
                // stack.
                await Task.Run(async () =>
                {
                    depTrunk.EnsureCapacity(package.Dependencies.Count);
                    await foreach (var dependency in deps)
                    {
                        resolvingStack.Push(dependency);
                        depTrunk.Add(dependency);
                    }
                });
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

    public async IAsyncEnumerable<PackageMeta> ResolveDependencies(PackageMeta meta, PackageIndex index)
    {
        if (meta.Dependencies == null)
        {
            yield break;
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

            yield return depMeta;
        }
    }
}
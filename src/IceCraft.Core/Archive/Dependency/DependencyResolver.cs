namespace IceCraft.Core.Archive.Dependency;

using System.Runtime.CompilerServices;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Installation;
using Microsoft.Extensions.Logging;
using Semver;

public class DependencyResolver : IDependencyResolver
{
    private readonly IPackageInstallManager _installManager;
    private readonly ILogger<DependencyResolver> _logger;

    private readonly record struct StackBranch
    {
        public StackBranch(PackageMeta package, int layer)
        {
            Package = package;
            Layer = layer;
        }

        internal PackageMeta Package { get; }
        internal int Layer { get; }
    }

    public DependencyResolver(IPackageInstallManager installManager,
        ILogger<DependencyResolver> logger)
    {
        _installManager = installManager;
        _logger = logger;
    }

    private static PackageMeta? SelectLatest(ParallelQuery<PackageMeta> packages, 
        CancellationToken cancellationToken = default)
    {
        PackageMeta? best = null;

        foreach (var meta in packages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (meta.Version.CompareSortOrderTo(best?.Version) > 0)
            {
                best = meta;
            }
        }

        return best;
    }

    public async Task ResolveTree(PackageMeta meta, PackageIndex index, ISet<PackageMeta> setToAppend, CancellationToken cancellationToken = default)
    {
        // The dependency tree trunk.
        var depTrunk = new HashSet<PackageMeta>(meta.Dependencies?.Count ?? 10);

        // Keep track of all parents in the dependency tree.
        var depTreeParents = new HashSet<StackBranch>(meta.Dependencies?.Count ?? 10);

        var resolvingStack = new Stack<StackBranch>();
        resolvingStack.Push(new StackBranch(meta, 0));

        // The current layer of dependencies.
        // Main branches are at layer 1.
        var layer = 0;

        const int mainBranchLayer = 1;
        
        // Step 1: generate dependency trees for all dependencies.
        await Task.Run(async () =>
        {
            while (resolvingStack.Count != 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var branch = resolvingStack.Pop();
                layer = branch.Layer;

                // Clear at the main branch layer.
                if (layer == mainBranchLayer)
                {
                    depTreeParents.Clear();
                }

                if (branch.Package.Dependencies == null || branch.Package.Dependencies.Count == 0)
                {
                    continue;
                }
                
                var deps = ResolveDependencies(branch.Package, index);
                
                // Add into dependency trunk and push the dependency package into the resolving
                // stack.
                await Task.Run(async () =>
                {
                    depTrunk.EnsureCapacity(branch.Package.Dependencies.Count);

                    // Whether the current branch will be a parent of a dependency branch.
                    var currentIsParent = false;
                    await foreach (var dependency in deps)
                    {
                        // Detect circular references to parents of the current parent.
                        if (depTreeParents.Where(x => x.Layer < layer)
                            .Any(x => x.Package.Id == dependency.Id))
                        {
                            throw DependencyException.Circular(dependency.Id, dependency.Version.ToString(), meta.Id);
                        }

                        currentIsParent = true;
                        cancellationToken.ThrowIfCancellationRequested();
                        
                        // Detect circular reference to current parent.
                        if (dependency.Dependencies?.Any(x => x.PackageId == meta.Id) == true)
                        {
                            throw DependencyException.Circular(dependency.Id, dependency.Version.ToString(), meta.Id);
                        }

                        resolvingStack.Push(new StackBranch(dependency, layer + 1));
                        depTrunk.Add(dependency);
                    }

                    if (currentIsParent)
                    {
                        depTreeParents.Add(branch);
                    }
                }, cancellationToken);
            }
        }, cancellationToken);
        
        // Faster logic for expandable lists.
        if (setToAppend is HashSet<PackageMeta> expandableList)
        {
            await Task.Run(() =>
            {
                expandableList.EnsureCapacity(expandableList.Count + depTrunk.Count);
                foreach (var entry in depTrunk)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    setToAppend.Add(entry);
                }
            }, cancellationToken);
            return;
        }

        // Much slower logic.
        _logger.LogWarning("ResolveTree was called with a set that cannot EnsureCapacity");
        _logger.LogWarning("Expect upcoming worsened performance");
        
        await Task.Run(() =>
        {
            foreach (var entry in depTrunk)
            {
                cancellationToken.ThrowIfCancellationRequested();
                setToAppend.Add(entry);
            }
        }, cancellationToken);
    }

    public async IAsyncEnumerable<PackageMeta> ResolveDependencies(PackageMeta meta, 
        PackageIndex index, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (meta.Dependencies == null)
        {
            yield break;
        }

        foreach (var dependency in meta.Dependencies)
        {
            cancellationToken.ThrowIfCancellationRequested();

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
            var depMeta = await SelectBestPackageDependency(seriesInfo.Versions.Values.Select(x => x.Metadata)
                , dependency, cancellationToken);

            if (depMeta.Dependencies?.Any(x => x.PackageId == meta.Id) == true)
            {
                throw DependencyException.Circular(dependency, meta.Id);
            }

            yield return depMeta;
        }
    }

    internal static async Task<PackageMeta> SelectBestPackageDependency(IEnumerable<PackageMeta> metas, 
        DependencyReference dependency, 
        CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
            metas
                .Where(x => dependency.VersionRange.Contains(x.Version))
                .AsParallel()
                .WithCancellation(cancellationToken)
                .MaxBy(x => x.Version, SemVersion.SortOrderComparer)
            ?? throw DependencyException.Unsatisfied(dependency), cancellationToken);
    }
}
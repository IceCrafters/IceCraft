// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Archive.Dependency;

using System.Runtime.CompilerServices;
using IceCraft.Api.Archive.Indexing;
using IceCraft.Api.Client;
using IceCraft.Api.Installation;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;
using Semver;

public class DependencyResolver : IDependencyResolver
{
    private readonly IPackageInstallManager _installManager;
    private readonly IOutputAdapter _output;

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
        IFrontendApp frontendApp)
    {
        _installManager = installManager;
        _output = frontendApp.Output;
    }

    public async Task ResolveTree(PackageMeta meta, PackageIndex index, ISet<DependencyLeaf> setToAppend, CancellationToken cancellationToken = default)
    {
        // The dependency tree trunk.
        var depTrunk = new HashSet<DependencyLeaf>(meta.Dependencies?.Count ?? 10);

        // Keep track of all parents in the dependency tree.
        var depTreeParents = new HashSet<StackBranch>(meta.Dependencies?.Count ?? 10);

        var resolvingStack = new Stack<StackBranch>();
        resolvingStack.Push(new StackBranch(meta, 0));

        // The current layer of dependencies.
        // Main branches are at layer 1.
        int layer;

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
                
                var deps = ResolveDependencies(branch.Package, index, cancellationToken);
                
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
                        var layer1 = layer;
                        if (depTreeParents.Where(x => x.Layer < layer1)
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
                        depTrunk.Add(new DependencyLeaf(dependency, false));
                    }

                    if (currentIsParent)
                    {
                        depTreeParents.Add(branch);
                    }
                }, cancellationToken);
            }
        }, cancellationToken);
        
        // Faster logic for expandable lists.
        if (setToAppend is HashSet<DependencyLeaf> expandableList)
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
        _output.Warning("ResolveTree was called with a set that cannot EnsureCapacity");
        _output.Warning("Expect upcoming worsened performance");
        
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

     internal static async Task<PackageMeta?> SelectBestPackageDependencyOrDefault(IEnumerable<PackageMeta> metas, 
        DependencyReference dependency, 
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
            metas
                .Where(x => dependency.PackageId.Equals(x.Id) && dependency.VersionRange.Contains(x.Version))
                .AsParallel()
                .WithCancellation(cancellationToken)
                .MaxBy(x => x.Version, SemVersion.SortOrderComparer), cancellationToken);
    }

    internal static async Task<PackageMeta> SelectBestPackageDependency(IEnumerable<PackageMeta> metas, 
        DependencyReference dependency, 
        CancellationToken cancellationToken = default)
    {
        return await SelectBestPackageDependencyOrDefault(metas, dependency, cancellationToken)
            ?? throw DependencyException.Unsatisfied(dependency);
    }
}
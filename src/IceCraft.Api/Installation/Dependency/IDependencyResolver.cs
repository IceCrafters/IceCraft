// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation.Dependency;

using IceCraft.Api.Archive.Indexing;
using IceCraft.Api.Package;

public interface IDependencyResolver
{
    /// <summary>
    /// Resolves an entire tree of all dependencies, and appends the resolved dependencies into the specified
    /// set top-down (i.e. all entries flattened and inserted in a visually top-to-down order if dependencies
    /// are represented in trees).
    /// </summary>
    /// <param name="meta">The package to resolve dependencies for.</param>
    /// <param name="index">The package index to resolve dependencies from.</param>
    /// <param name="setToAppend">The set to append dependencies from. A growable list like <see cref="HashSet{T}"/> is recommended.</param>
    /// <param name="cancellationToken">The token to cancel the operation.</param>
    /// <exception cref="DependencyException">Dependency is either invalid or cannot be satisfied.</exception>
    /// <exception cref="OperationCanceledException">Operation is cancelled.</exception>
    Task ResolveTree(PackageMeta meta, PackageIndex index, ISet<DependencyLeaf> setToAppend, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a single layer of dependencies.
    /// </summary>
    /// <param name="meta">The package to resolve dependencies for.</param>
    /// <param name="index">The package index to resolve dependencies from.</param>
    /// <param name="cancellationToken">The token to cancel the operation.</param>
    /// <exception cref="DependencyException">Dependency is either invalid or cannot be satisfied.</exception>
    /// <exception cref="OperationCanceledException">Operation is cancelled.</exception>
    IAsyncEnumerable<PackageMeta> ResolveDependencies(PackageMeta meta, PackageIndex index, CancellationToken cancellationToken = default);
}
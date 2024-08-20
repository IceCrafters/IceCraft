namespace IceCraft.Core.Archive.Dependency;

using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Packaging;

public interface IDependencyResolver
{
    /// <summary>
    /// Resolves an entire tree of all dependencies.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implementations of this method will likely use recursion. If the dependency tree has too many branches,
    /// the stack may overflow.
    /// </para>
    /// </remarks>
    /// <param name="meta">The package to resolve dependencies for.</param>
    /// <param name="index">The package index to resolve dependencies from.</param>
    /// <param name="setToAppend">The set to append dependencies from. A growable list like <see cref="HashSet{T}"/> is recommended.</param>
    /// <exception cref="DependencyException">Dependency is either invalid or cannot be satisfied.</exception>
    Task ResolveTree(PackageMeta meta, PackageIndex index, ISet<PackageMeta> setToAppend);

    /// <summary>
    /// Resolves a single layer of dependencies.
    /// </summary>
    /// <param name="meta">The package to resolve dependencies for.</param>
    /// <param name="index">The package index to resolve dependencies from.</param>
    IAsyncEnumerable<PackageMeta> ResolveDependencies(PackageMeta meta, PackageIndex index);
}
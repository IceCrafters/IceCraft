namespace IceCraft.Core.Installation.Analysis;

using System.Diagnostics.Contracts;
using IceCraft.Core.Archive.Dependency;
using IceCraft.Core.Archive.Packaging;

public interface IDependencyMapper
{
    [Pure]
    Task<DependencyMap> MapDependencies();

    [Pure]
    IAsyncEnumerable<DependencyReference> MapUnmetDependencies();

    [Pure]
    IAsyncEnumerable<PackageMeta> EnumerateUnsatisifiedPackages();

    Task<DependencyMap> MapDependenciesCached();
}
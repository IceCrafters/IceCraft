namespace IceCraft.Api.Installation.Dependency;

using System.Diagnostics.Contracts;
using IceCraft.Api.Package;

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
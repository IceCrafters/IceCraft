namespace IceCraft.Core.Installation.Analysis;

using System.Diagnostics.Contracts;

public interface IDependencyMapper
{
    [Pure]
    Task<DependencyMap> MapDependencies();
    Task<DependencyMap> MapDependenciesCached();
}
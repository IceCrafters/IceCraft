namespace IceCraft.Core.Installation.Analysis;

public interface IDependencyMapper
{
    Task<DependencyMap> MapDependencies();
    Task<DependencyMap> MapDependenciesCached();
}
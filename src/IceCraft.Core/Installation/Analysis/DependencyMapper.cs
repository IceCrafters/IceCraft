namespace IceCraft.Core.Installation.Analysis;

using IceCraft.Core.Archive.Dependency;
using IceCraft.Core.Caching;
using IceCraft.Core.Installation.Storage;
using IceCraft.Core.Platform;
using IceCraft.Core.Serialization;
using Microsoft.Extensions.Logging;

public class DependencyMapper : IDependencyMapper, ICacheClearable
{
    private static readonly Guid CacheGuid = new("478DA958-DADF-4789-B05A-B63A095D97D6");
    private const string CacheObjectName = "dependencyMap";
    
    private readonly IPackageInstallDatabaseFactory _databaseFactory;
    private readonly IOutputAdapter _output;
    private readonly ICacheStorage _cacheStorage;

    public DependencyMapper(IPackageInstallDatabaseFactory databaseFactory,
        IFrontendApp frontendApp,
        ICacheManager cacheManager)
    {
        _databaseFactory = databaseFactory;
        _output = frontendApp.Output;
        _cacheStorage = cacheManager.GetStorage(CacheGuid);
    }

    public async Task<DependencyMap> MapDependencies()
    {
        var database = await _databaseFactory.GetAsync();
        var map = new DependencyMap(database.Count);

        await Task.Run(async () =>
        {
            foreach (var (key, index) in database)
            {
                await ProcessIndex(key, database, index, map);
            }
        });

        return map;
    }

    public async Task<DependencyMap> MapDependenciesCached()
    {
        return await _cacheStorage.RollJsonAsync(CacheObjectName, 
            MapDependencies,
            IceCraftCoreContext.Default.DependencyMap);
    }

    private async ValueTask ProcessIndex(string key, 
        IPackageInstallDatabase database, 
        PackageInstallationIndex index, 
        DependencyMap map)
    {
        var branch = new DependencyMapBranch(index.Count);
        
        foreach (var (version, info) in index)
        {
            var entry = map.GetEntry(info.Metadata);

            if (info.Metadata.Dependencies == null)
            {
                continue;
            }
            
            foreach (var dependency in info.Metadata.Dependencies)
            {
                var best = await DependencyResolver.SelectBestPackageDependency(database.EnumeratePackages(),
                    dependency,
                    default);

                var bestEntry = map.GetEntry(best);

                entry.Dependencies.Add(new PackageReference(best.Id,
                    best.Version));
                bestEntry.Dependents.Add(new PackageReference(info.Metadata.Id,
                    info.Metadata.Version));
            }
        }
    }

    public void ClearCache()
    {
        _cacheStorage.DeleteObject(CacheObjectName);
    }
}
namespace IceCraft.Core.Archive.Indexing;

using System.Threading.Tasks;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Caching;
using IceCraft.Core.Serialization;

public class CachedIndexer : IPackageIndexer, ICacheClearable
{
    private static readonly Guid CacheStorageId = new("5ce7b9d1-0aea-4aa1-bb9d-e713c457c632");
    private const string PackageIndexStorage = "pkgIndex";

    private readonly ICacheManager _cacheManager;
    private readonly ICacheStorage _cacheStorage;

    public CachedIndexer(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;

        _cacheStorage = _cacheManager.GetStorage(CacheStorageId);
    }

    public async Task<PackageIndex> IndexAsync(IRepositorySourceManager manager)
    {
        var dict = await _cacheStorage.RollJsonAsync(PackageIndexStorage, 
            async () => await GenerateNewIndex(manager),
            IceCraftCoreContext.Default.BasePackageIndex);

        return new PackageIndex(dict);
    }

    private static async Task<Dictionary<string, PackageMeta>> GenerateNewIndex(IRepositorySourceManager manager)
    {
        var index = new Dictionary<string, PackageMeta>(manager.Count);
        var repos = await manager.GetRepositoriesAsync();
        
        foreach (var repo in repos)
        {
            index.EnsureCapacity(index.Count + repo.GetExpectedSeriesCount());
            var series = repo.EnumerateSeries();
            
            foreach (var x in series)
            {
                var latest = await x.GetLatestAsync();
                if (latest == null)
                {
                    continue;
                }

                var meta = latest.GetMeta();
                index.Add(x.Name, meta);
            }
        }

        return index;
    }

    public void ClearCache()
    {
        _cacheStorage.DeleteObject(PackageIndexStorage);
    }
}

namespace IceCraft.Repositories.Adoptium;

using IceCraft.Core.Archive;
using IceCraft.Core.Archive.Providers;
using IceCraft.Core.Caching;
using IceCraft.Repositories.Adoptium.Models;

public class AdoptiumRepositoryProvider : IRepositoryProvider
{
    private static readonly Guid StorageGuid = new("ad2c3cc6-4ad4-4c7a-bb45-cd3c85cea041");
    private const string AvailableReleaseCacheId = "available_releases";

    private readonly AdoptiumApiClient _client = new();
    private readonly ICacheManager _cacheManager;
    private readonly ICacheStorage _cacheStorage;

    public AdoptiumRepositoryProvider(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
        _cacheStorage = _cacheManager.GetStorage(StorageGuid);
    }

    private async Task<IRepository?> CreateRepositoryInternal(bool regenerate)
    {
        var releases = await _cacheStorage.RollJsonAsync(AvailableReleaseCacheId,
            _client.GetAvailableReleasesAsync, 
            null, 
            regenerate);

        if (releases == null)
        {
            return null;
        }

        return new AdoptiumRepository(releases, this);
    }

    public async Task<IRepository?> CreateRepository()
    {
        return await CreateRepositoryInternal(false);
    }

    public async Task<IRepository?> RegenerateRepository()
    {
        return await CreateRepositoryInternal(true);
    }
}

namespace IceCraft.Repositories.Adoptium;

using IceCraft.Core.Archive;
using IceCraft.Core.Archive.Providers;
using IceCraft.Core.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class AdoptiumRepositoryProvider : IRepositorySource
{
    private static readonly Guid StorageGuid = new("ad2c3cc6-4ad4-4c7a-bb45-cd3c85cea041");
    private const string AvailableReleaseCacheId = "available_releases";

    private readonly ICacheManager _cacheManager;

    public AdoptiumRepositoryProvider(IServiceProvider provider)
    {
        _cacheManager = provider.GetRequiredService<ICacheManager>();
        CacheStorage = _cacheManager.GetStorage(StorageGuid);
        Client = new(provider.GetRequiredService<ILogger<AdoptiumRepositoryProvider>>());
    }

    internal AdoptiumApiClient Client { get; }

    internal ICacheStorage CacheStorage { get; }

    private async Task<IRepository?> CreateRepositoryInternal(bool regenerate)
    {
        var releases = await CacheStorage.RollJsonAsync(AvailableReleaseCacheId,
            Client.GetAvailableReleasesAsync, 
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

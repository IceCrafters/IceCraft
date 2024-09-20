namespace IceCraft.Extensions.CentralRepo.Impl;

using System.IO.Abstractions;
using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Caching;
using IceCraft.Api.Client;
using IceCraft.Extensions.CentralRepo.Network;

public class RemoteRepositorySource : IRepositorySource
{
    private readonly IFrontendApp _frontendApp;
    private readonly HttpClient _httpClient;
    private readonly ICacheStorage _cacheStorage;
    private readonly RemoteRepositoryManager _remoteManager;
    private readonly RemoteRepositoryIndexer _repositoryIndexer;

    private const string StorageUuid = "E5EFB74F-6F93-42C1-85D6-B15A9556B647";
    private const string IndexCacheObj = "remoteIndex";

    public RemoteRepositorySource(IFrontendApp frontendApp, 
        ICacheManager cacheManager,
        RemoteRepositoryManager remoteManager, 
        RemoteRepositoryIndexer repositoryIndexer)
    {
        _frontendApp = frontendApp;
        _remoteManager = remoteManager;
        _repositoryIndexer = repositoryIndexer;
        _httpClient = _frontendApp.GetClient();

        _cacheStorage = cacheManager.GetStorage(new Guid(StorageUuid));
    }
    
    public async Task<IRepository?> CreateRepositoryAsync()
    {
        await _remoteManager.InitializeCacheAsync();
        var (count, series) = await _repositoryIndexer.IndexSeries();

        return new RemoteRepository(series, count);
    }

    public Task RefreshAsync()
    {
        _remoteManager.CleanPrevious();
        return Task.CompletedTask;
    }
}
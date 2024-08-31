namespace IceCraft.Extensions.CentralRepo.Impl;

using System.Net.Http.Json;
using IceCraft.Core.Archive;
using IceCraft.Core.Archive.Providers;
using IceCraft.Core.Caching;
using IceCraft.Core.Platform;
using IceCraft.Extensions.CentralRepo.Models;

public class RemoteRepositorySource : IRepositorySource
{
    private readonly IFrontendApp _frontendApp;
    private readonly HttpClient _httpClient;
    private readonly ICacheStorage _cacheStorage;

    private const string StorageUuid = "E5EFB74F-6F93-42C1-85D6-B15A9556B647";
    private const string IndexCacheObj = "remoteIndex";

    public RemoteRepositorySource(IFrontendApp frontendApp, ICacheManager cacheManager)
    {
        _frontendApp = frontendApp;
        _httpClient = _frontendApp.GetClient();

        _cacheStorage = cacheManager.GetStorage(new Guid(StorageUuid));
    }
    
    private static string GetConfigureRepoUrl()
    {
        return Environment.GetEnvironmentVariable("ICECRAFT_CSR_INDEX_URL")
               ?? ""; // TODO do real CSR
    }
    
    public async Task<IRepository?> CreateRepositoryAsync()
    {
        var indexUrl = GetConfigureRepoUrl();
        if (string.IsNullOrWhiteSpace(indexUrl))
        {
            return null;
        }
        
        var index = await _cacheStorage.RollJsonAsync(IndexCacheObj, 
            async () => await _httpClient.GetFromJsonAsync<RemoteIndex>(indexUrl));
        
        return index == null
            ? null
            : new RemoteRepository(index);
    }

    public Task RefreshAsync()
    {
        _cacheStorage.DeleteObject(IndexCacheObj);
        return Task.CompletedTask;
    }
}
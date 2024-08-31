namespace IceCraft.Extensions.CentralRepo.Impl;

using System.Net.Http.Json;
using IceCraft.Core.Archive;
using IceCraft.Core.Archive.Providers;
using IceCraft.Core.Platform;
using IceCraft.Extensions.CentralRepo.Models;

public class RemoteRepositorySource : IRepositorySource
{
    private readonly IFrontendApp _frontendApp;
    private readonly HttpClient _httpClient;

    public RemoteRepositorySource(IFrontendApp frontendApp)
    {
        _frontendApp = frontendApp;
        _httpClient = _frontendApp.GetClient();
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

        var index = await _httpClient.GetFromJsonAsync<RemoteIndex>(indexUrl);
        
        return index == null
            ? null
            : new RemoteRepository(index);
    }

    public Task RefreshAsync()
    {
        throw new NotImplementedException();
    }
}
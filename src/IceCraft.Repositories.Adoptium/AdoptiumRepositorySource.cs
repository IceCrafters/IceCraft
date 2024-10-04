// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Repositories.Adoptium;

using System.Collections.Generic;
using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Caching;
using IceCraft.Core.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class AdoptiumRepositorySource : AsyncRepositorySource
{
    private static readonly Guid StorageGuid = new("ad2c3cc6-4ad4-4c7a-bb45-cd3c85cea041");
    private const string AvailableReleaseCacheId = "available_releases";

    private readonly ICacheManager _cacheManager;
    private readonly ILogger _logger;

    public AdoptiumRepositorySource(IServiceProvider provider)
    {
        _cacheManager = provider.GetRequiredService<ICacheManager>();
        CacheStorage = _cacheManager.GetStorage(StorageGuid);
        _logger = provider.GetRequiredService<ILogger<AdoptiumRepositorySource>>();
        Client = new(_logger);
    }

    internal AdoptiumApiClient Client { get; }

    internal ICacheStorage CacheStorage { get; }

    private async Task<IRepository?> CreateRepositoryInternal(bool regenerate)
    {
        var releases = await CacheStorage.RollJsonAsync(AvailableReleaseCacheId,
            Client.GetAvailableReleasesAsync,
            reset: regenerate);

        if (releases == null)
        {
            return null;
        }
        _logger.LogTrace("Adoptium: {Count} releases", releases.AvailableReleases.Count);

        return new AdoptiumRepository(releases, this, _logger);
    }

    public override Task RefreshAsync()
    {
        // Clears all cache storage objects so all latest information, etc.
        // are regenerated.
        CacheStorage.Clear();

        return Task.CompletedTask;
    }

    public override async IAsyncEnumerable<RepositoryInfo> CreateRepositoriesAsync()
    {
        var repository = await CreateRepositoryInternal(false);
        if (repository != null)
        {
            yield return new RepositoryInfo("adoptium", repository);
        }
    }
}

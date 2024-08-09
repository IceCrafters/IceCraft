﻿namespace IceCraft.Repositories.Adoptium;

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using IceCraft.Core.Archive;
using IceCraft.Core.Caching;
using IceCraft.Repositories.Adoptium.Models;
using Microsoft.Extensions.Logging;

public class AdoptiumPackageSeries : IPackageSeries
{
    private readonly int _majorVersion;
    private readonly string _type;
    private readonly AdoptiumRepository _repository;
    private readonly ILogger _logger;

    internal AdoptiumPackageSeries(int majorVersion, string type, AdoptiumRepository repository, ILogger logger)
    {
        _majorVersion = majorVersion;
        _type = type;
        Name = $"adoptium{_majorVersion}-{type}";
        _repository = repository;
        _logger = logger;
    }

    public string Name { get; }

    public async Task<IEnumerable<IPackage>> EnumeratePackagesAsync()
    {
        if (!AdoptiumApiClient.IsArchitectureSupported(RuntimeInformation.OSArchitecture))
        {
            _logger.LogWarning("Architecture not supported");
            // Architecture not supported.
            return [];
        }
        var all = await GetAllAssetViewsAsync();

        // ReSharper disable once InvertIf
        if (all == null)
        {
            // Malformed upstream
            _logger.LogWarning("Adoptium API returned no valid versions conforming to required pattern");
            _logger.LogWarning("Not providing packages");
            return [];
        }

        return all
            .Select(x => new AdoptiumPackage(this, x, _logger));
    }

    private async Task<IEnumerable<AdoptiumBinaryRelease>?> GetAllAssetViewsAsync()
    {
        return await _repository.Provider.CacheStorage.RollJsonAsync($"{Name}.all",
            async () => await _repository.Provider.Client.GetFeatureReleasesAsync(_majorVersion,
            "ga",
            RuntimeInformation.OSArchitecture,
            _type,
            "hotspot",
            AdoptiumApiClient.GetOs()));
    }

    public async Task<int> GetExpectedPackageCountAsync()
    {
        var views = await GetAllAssetViewsAsync();
        return views?.Count() ?? 0;
    }

    public Task<IPackage?> GetLatestAsync()
    {
        _logger.LogWarning("Unsupported API call GetLatestAsync");
        return Task.FromResult<IPackage?>(null);
    }

    private async Task<AdoptiumBinaryAssetView?> GetLatestAssetView()
    {
        if (!AdoptiumApiClient.IsArchitectureSupported(RuntimeInformation.OSArchitecture)
            || !AdoptiumApiClient.IsOsSupported())
        {
            // Architecture/OS not supported.
            return null;
        }

        _logger.LogTrace("Getting a hotspot Java {MajorVersion} '{Type}' for '{OS}' '{OSArchitecture}'",
            _majorVersion,
            _type,
            AdoptiumApiClient.GetOs(),
            RuntimeInformation.OSArchitecture);

        return await _repository.Provider.CacheStorage.RollJsonAsync($"{Name}.latest",
            async () => (await _repository.Provider.Client.GetLatestReleaseAsync(_majorVersion,
            "hotspot",
            RuntimeInformation.OSArchitecture,
            _type,
            AdoptiumApiClient.GetOs()))?.FirstOrDefault(x
                => x is { Binary: not null }));
    }

    public async Task<string?> GetLatestVersionIdAsync()
    {
        var view = await GetLatestAssetView();
        if (view is not { Version: not null })
        {
            _logger.LogWarning("Adoptium latest release ('{ReleaseName}') comes without a version", view?.ReleaseName);
            return null;
        }

        return view.Version.Semver;
    }
}

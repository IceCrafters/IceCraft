namespace IceCraft.Repositories.Adoptium;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using IceCraft.Core.Archive;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Caching;
using IceCraft.Repositories.Adoptium.Models;
using Microsoft.Extensions.Logging;
using Semver;

public class AdoptiumPackageSeries : AsyncPackageSeries
{
    private readonly int _majorVersion;
    private readonly AdoptiumRepository _repository;
    private readonly ILogger _logger;

    internal AdoptiumPackageSeries(int majorVersion, string type, AdoptiumRepository repository, ILogger logger)
    {
        _majorVersion = majorVersion;
        Type = type;
        Name = $"adoptium{_majorVersion}-{type}";
        _repository = repository;
        _logger = logger;
    }

    public override string Name { get; }

    public string Type { get; }

    public override async IAsyncEnumerable<IPackage> EnumeratePackagesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var latest = await GetLatestVersionIdAsync();

        if (!AdoptiumApiClient.IsArchitectureSupported(RuntimeInformation.OSArchitecture))
        {
            _logger.LogWarning("Architecture not supported");
            // Architecture not supported.
            yield break;
        }
        // TODO make this not IO bound
        var all = await GetAllAssetViewsAsync();

        // ReSharper disable once InvertIf
        if (all == null)
        {
            // Malformed upstream
            _logger.LogWarning("Adoptium API returned no valid versions conforming to required pattern");
            _logger.LogWarning("Not providing packages");
            yield break;
        }

        foreach (var release in all)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return new AdoptiumPackage(this, 
                release, 
                _logger, 
                latest != null 
                    && release.VersionData != null 
                    && SemVersion.Parse(release.VersionData.Semver, SemVersionStyles.Strict) == latest);
        }
    }

    private async Task<IEnumerable<AdoptiumBinaryRelease>?> GetAllAssetViewsAsync()
    {
        return await _repository.Provider.CacheStorage.RollJsonAsync($"{Name}.all",
            async () => await _repository.Provider.Client.GetFeatureReleasesAsync(_majorVersion,
            "ga",
            RuntimeInformation.OSArchitecture,
            Type,
            "hotspot",
            AdoptiumApiClient.GetOs()));
    }

    public override async Task<int> GetExpectedPackageCountAsync()
    {
        var views = await GetAllAssetViewsAsync();
        return views?.Count() ?? 0;
    }

    public override Task<IPackage?> GetLatestAsync()
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
            Type,
            AdoptiumApiClient.GetOs(),
            RuntimeInformation.OSArchitecture);

        return await _repository.Provider.CacheStorage.RollJsonAsync($"{Name}.latest",
            async () => (await _repository.Provider.Client.GetLatestReleaseAsync(_majorVersion,
            "hotspot",
            RuntimeInformation.OSArchitecture,
            Type,
            AdoptiumApiClient.GetOs()))?.FirstOrDefault(x
                => x is { Binary: not null }));
    }

    public override async Task<SemVersion?> GetLatestVersionIdAsync()
    {
        var view = await GetLatestAssetView();
        if (view is not { Version: not null })
        {
            _logger.LogWarning("Adoptium latest release ('{ReleaseName}') comes without a version", view?.ReleaseName);
            return null;
        }

        return SemVersion.Parse(view.Version.Semver, SemVersionStyles.Strict);
    }
}

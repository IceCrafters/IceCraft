namespace IceCraft.Repositories.Adoptium;

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using IceCraft.Core.Archive;
using IceCraft.Core.Caching;
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
            // Architecture not supported.
            return [];
        }

        var all = await _repository.Provider.CacheStorage.RollJsonAsync($"{Name}.all",
            async () => await _repository.Provider.Client.GetFeatureReleasesAsync(_majorVersion,
            "ga",
            RuntimeInformation.OSArchitecture,
            _type,
            "hotspot",
            AdoptiumApiClient.GetOs()));

        // ReSharper disable once InvertIf
        if (all == null)
        {
            // Malformed upstream
            _logger.LogWarning("Adoptium API returned no valid versions conforming to required pattern");
            _logger.LogWarning("Not providing packages");
            return [];
        }

        return all
            .Where(x => x.Binary is { Package: not null })
            .Select(x => new AdoptiumPackage(this, x));
    }

    public async Task<IPackage?> GetLatestAsync()
    {
        if (!AdoptiumApiClient.IsArchitectureSupported(RuntimeInformation.OSArchitecture)
            || !AdoptiumApiClient.IsOsSupported())
        {
            // Architecture/OS not supported.
            return null;
        }

        var latest = await _repository.Provider.CacheStorage.RollJsonAsync($"{Name}.latest",
            async () => (await _repository.Provider.Client.GetLatestReleaseAsync(_majorVersion,
            "hotspot",
            RuntimeInformation.OSArchitecture,
            _type,
            AdoptiumApiClient.GetOs()))?.FirstOrDefault(x
                => x is { Binary.Package: not null }));

        return latest != null
            ? new AdoptiumPackage(this, latest)
            : null;
    }
}

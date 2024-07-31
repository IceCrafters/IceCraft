namespace IceCraft.Repositories.Adoptium;

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using IceCraft.Core.Archive;
using IceCraft.Core.Caching;

public class AdoptiumPackageSeries : IPackageSeries
{
    private readonly int _majorVersion;
    private readonly string _type;
    private readonly AdoptiumRepository _repository;

    internal AdoptiumPackageSeries(int majorVersion, string type, AdoptiumRepository repository)
    {
        _majorVersion = majorVersion;
        _type = type;
        Name = $"adoptium{_majorVersion}-{type}";
        _repository = repository;
    }

    public string Name { get; }

    public Task<IEnumerable<IPackage>> EnumeratePackagesAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IPackage?> GetLatestAsync()
    {
        if (!AdoptiumApiClient.IsArchitectureSupported(RuntimeInformation.OSArchitecture))
        {
            // Architecture not supported.
            return null;
        }

        var latest = await _repository.Provider.CacheStorage.RollJsonAsync($"{Name}.latest",
            async () => (await _repository.Provider.Client.GetLatestReleaseAsync(_majorVersion,
            "hotspot",
            RuntimeInformation.OSArchitecture,
            _type))?.FirstOrDefault());
        if (latest == null)
        {
            // No version available.
            return null;
        }

        if (latest.Binary == null || latest.Binary.Package == null)
        {
            // Missing binary.
            Console.WriteLine("Warning: adoptium: Missing binary for package {0}", Name);
            return null;
        }

        return new AdoptiumPackage(this, latest);
    }
}

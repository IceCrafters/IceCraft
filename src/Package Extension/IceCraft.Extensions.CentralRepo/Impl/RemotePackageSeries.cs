namespace IceCraft.Extensions.CentralRepo.Impl;

using IceCraft.Core.Archive;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Extensions.CentralRepo.Models;
using Semver;

public class RemotePackageSeries : IPackageSeries
{
    private readonly RemoteSeriesEntry _seriesEntry;

    public RemotePackageSeries(string id, RemoteSeriesEntry seriesEntry)
    {
        Name = id;
        _seriesEntry = seriesEntry;
    }

    public string Name { get; }
    
    public PackageTranscript? Transcript => _seriesEntry.Transcript;

    public Task<IPackage?> GetLatestAsync()
    {
        return Task.FromResult<IPackage?>(null);
    }

    public IEnumerable<IPackage> EnumeratePackages(CancellationToken cancellationToken = default)
    {
        foreach (var entry in _seriesEntry.Versions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return new RemotePackage(entry, this, Name);
        }
    }

    public Task<SemVersion?> GetLatestVersionIdAsync()
    {
        return Task.FromResult<SemVersion?>(null);
    }

    public Task<int> GetExpectedPackageCountAsync()
    {
        return Task.FromResult(_seriesEntry.Versions.Count);
    }
}
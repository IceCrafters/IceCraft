namespace IceCraft.Extensions.CentralRepo.Impl;

using IceCraft.Api.Archive.Repositories;
using IceCraft.Core.Archive;
using IceCraft.Extensions.CentralRepo.Models;

public class RemoteRepository : IRepository
{
    private readonly RemoteIndex _remoteIndex;

    public RemoteRepository(RemoteIndex remoteIndex)
    {
        _remoteIndex = remoteIndex;
    }
    
    [Obsolete("Always return null.")]
    public IPackageSeries? GetSeriesOrDefault(string name)
    {
        return null;
    }

    public IEnumerable<IPackageSeries> EnumerateSeries()
    {
        foreach (var (key, series) in _remoteIndex)
        {
            yield return new RemotePackageSeries(key, series);
        }
    }

    public int GetExpectedSeriesCount()
    {
        return _remoteIndex.Count;
    }
}
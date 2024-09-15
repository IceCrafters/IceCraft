namespace IceCraft.Extensions.CentralRepo.Impl;

using IceCraft.Api.Archive.Repositories;
using IceCraft.Core.Archive;
using IceCraft.Extensions.CentralRepo.Models;
using IceCraft.Extensions.CentralRepo.Network;

public class RemoteRepository : IRepository
{
    private readonly IEnumerable<RemotePackageSeries> _series;
    private readonly int _count;

    public RemoteRepository(IEnumerable<RemotePackageSeries> series, int count)
    {
        _series = series;
        _count = count;
    }
    
    [Obsolete("Always return null.")]
    public IPackageSeries? GetSeriesOrDefault(string name)
    {
        return null;
    }

    public IEnumerable<IPackageSeries> EnumerateSeries()
    {
        return _series;
    }

    public int GetExpectedSeriesCount()
    {
        return _count;
    }
}
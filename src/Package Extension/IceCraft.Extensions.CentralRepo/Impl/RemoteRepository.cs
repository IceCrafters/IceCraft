// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Impl;

using IceCraft.Api.Archive.Repositories;

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
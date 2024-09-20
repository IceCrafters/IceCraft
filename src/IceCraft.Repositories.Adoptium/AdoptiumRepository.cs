// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Repositories.Adoptium;

using System.Collections.Generic;
using IceCraft.Api.Archive.Repositories;
using IceCraft.Repositories.Adoptium.Models;
using Microsoft.Extensions.Logging;

public class AdoptiumRepository : IRepository
{
    private readonly AvailableReleaseInfo _info;
    private readonly Dictionary<string, AdoptiumPackageSeries> _series;

    internal AdoptiumRepository(AvailableReleaseInfo releaseInfo, AdoptiumRepositorySource provider,
        ILogger logger)
    {
        Provider = provider;
        _info = releaseInfo;
        _series = new Dictionary<string, AdoptiumPackageSeries>(releaseInfo.AvailableReleases.Count * 2);
        foreach (var release in releaseInfo.AvailableReleases)
        {
            var jdk = new AdoptiumPackageSeries(release, "jdk", this, logger);
            var jre = new AdoptiumPackageSeries(release, "jre", this, logger);

            _series.Add(jdk.Name, jdk);
            _series.Add(jre.Name, jre);
        }
    }

    internal AdoptiumRepositorySource Provider { get; }

    public IEnumerable<IPackageSeries> EnumerateSeries()
    {
        return _series.Values;
    }

    public int GetExpectedSeriesCount()
    {
        return _series.Count;
    }

    public IPackageSeries? GetSeriesOrDefault(string name)
    {
        return _series.GetValueOrDefault(name);
    }
}

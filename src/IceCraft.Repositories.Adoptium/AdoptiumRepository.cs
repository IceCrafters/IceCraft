namespace IceCraft.Repositories.Adoptium;

using System.Collections.Generic;
using IceCraft.Core.Archive;
using IceCraft.Repositories.Adoptium.Models;

public class AdoptiumRepository : IRepository
{
    private readonly AvailableReleaseInfo _info;
    private readonly Dictionary<string, AdoptiumPackageSeries> _series;

    internal AdoptiumRepository(AvailableReleaseInfo releaseInfo, AdoptiumRepositoryProvider provider)
    {
        Provider = provider;
        _info = releaseInfo;
        _series = new(releaseInfo.AvailableReleases.Count * 2);
        foreach (var release in releaseInfo.AvailableReleases)
        {
            var jdk = new AdoptiumPackageSeries(release, "jdk", this);
            var jre = new AdoptiumPackageSeries(release, "jre", this);

            _series.Add(jdk.Name, jdk);
            _series.Add(jre.Name, jre);
        }
    }

    internal AdoptiumRepositoryProvider Provider { get; }

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

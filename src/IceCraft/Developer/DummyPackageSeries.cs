namespace IceCraft.Developer;

using System.Collections.Generic;
using System.Threading.Tasks;
using IceCraft.Core.Archive;
using Semver;

public class DummyPackageSeries : IPackageSeries
{
    public string Name { get; }
    private readonly DummyPackageBuilder[] _packages;

    public DummyPackageSeries(string name, DummyPackageBuilder[] packages)
    {
        _packages = packages;
        Name = name;
    }

    public Task<IEnumerable<IPackage>> EnumeratePackagesAsync()
    {
        return Task.FromResult(_packages.Select(x => (IPackage)x.Build(this)));
    }

    public Task<int> GetExpectedPackageCountAsync()
    {
        return Task.FromResult(_packages.Length);
    }

    public Task<IPackage?> GetLatestAsync()
    {
        return Task.FromResult<IPackage?>(null);
    }

    public Task<SemVersion?> GetLatestVersionIdAsync()
    {
        return Task.FromResult<SemVersion?>(null);
    }
}

namespace IceCraft.Developer;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using IceCraft.Core.Archive;
using Semver;

public class DummyPackageSeries : IPackageSeries
{
    public string Name => "dummy-test";
    private static readonly SemVersion[] _packageVersions = 
    [
        new(0, 1, 0, null, ["dummy1"]),
        new(0, 1, 1, null, ["dummy2"]),
        new(0, 2, 0, ["beta"], ["dummy3"])
    ];

    public Task<IEnumerable<IPackage>> EnumeratePackagesAsync()
    {
        return Task.FromResult(_packageVersions.Select(x => (IPackage)new DummyPackage(this, x)));
    }

    public Task<int> GetExpectedPackageCountAsync()
    {
        return Task.FromResult(1);
    }

    public Task<IPackage?> GetLatestAsync()
    {
        var sorted = _packageVersions.ToImmutableList();

        return Task.FromResult<IPackage?>(new DummyPackage(this, sorted[_packageVersions.Length - 1]));
    }

    public Task<SemVersion?> GetLatestVersionIdAsync()
    {
        var sorted = _packageVersions.ToImmutableList();

        return Task.FromResult<SemVersion?>(sorted[_packageVersions.Length - 1]);
    }
}

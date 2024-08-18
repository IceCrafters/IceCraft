namespace IceCraft.Developer;

using System.Collections.Generic;
using System.Threading.Tasks;
using IceCraft.Core.Archive;

public class DummyPackageSeries : IPackageSeries
{
    public string Name => "dummy-test";

    public Task<IEnumerable<IPackage>> EnumeratePackagesAsync()
    {
        return Task.FromResult<IEnumerable<IPackage>>([new DummyPackage(this)]);
    }

    public Task<int> GetExpectedPackageCountAsync()
    {
        return Task.FromResult(1);
    }

    public Task<IPackage?> GetLatestAsync()
    {
        return Task.FromResult<IPackage?>(new DummyPackage(this));
    }

    public Task<string?> GetLatestVersionIdAsync()
    {
        return Task.FromResult<string?>("0.1.0+dummy");
    }
}

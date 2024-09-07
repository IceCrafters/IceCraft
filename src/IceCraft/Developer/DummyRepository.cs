namespace IceCraft.Developer;

using System.Collections.Generic;
using IceCraft.Core.Archive;
using Semver;

public class DummyRepository : IRepository
{
    private static readonly Dictionary<string, IPackageSeries> SeriesMap = new()
    {
        { 
            "dummy-test", new DummyPackageSeries("dummy-test",
            [
                new DummyPackageBuilder()
                    .WithVersion(new(0, 1, 0, null, ["dummy1"])),
                new DummyPackageBuilder()
                    .WithVersion(new(0, 1, 1, null, ["dummy2"])),
                new DummyPackageBuilder()
                    .WithVersion(new(0, 2, 0, ["beta"], ["dummy3"]))
            ])
        },
        {
            "dummy-lib", new DummyPackageSeries("dummy-lib",
            [
                new DummyPackageBuilder()
                    .WithVersion(new(0, 1, 0)),
            ])
        },
        {
            "dummy-app", new DummyPackageSeries("dummy-app",
            [
                new DummyPackageBuilder()
                    .WithVersion(new(0, 1, 0))
                    .WithDependency("dummy-lib", SemVersionRange.AtLeast(new(0, 1, 0))),
            ])
        },
        {
            "dummy-app-unitary", new DummyPackageSeries("dummy-app-unitary",
            [
                new DummyPackageBuilder()
                    .WithVersion(new(0, 1, 0))
                    .Unitary(),
                new DummyPackageBuilder()
                    .WithVersion(new(0, 2, 0))
                    .Unitary()
            ])
        }
    };

    public IEnumerable<IPackageSeries> EnumerateSeries()
    {
        return SeriesMap.Values;
    }

    public int GetExpectedSeriesCount()
    {
        return 1;
    }

    public IPackageSeries? GetSeriesOrDefault(string name)
    {
        return SeriesMap.GetValueOrDefault(name);
    }
}

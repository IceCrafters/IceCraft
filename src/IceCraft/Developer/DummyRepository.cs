namespace IceCraft.Developer;

using System.Collections.Generic;
using IceCraft.Core.Archive;

public class DummyRepository : IRepository
{
    public IEnumerable<IPackageSeries> EnumerateSeries()
    {
        return [new DummyPackageSeries()];
    }

    public int GetExpectedSeriesCount()
    {
        return 1;
    }

    public IPackageSeries? GetSeriesOrDefault(string name)
    {
        if (name != "dummy-test")
        {
            return null;
        }

        return new DummyPackageSeries();
    }
}

namespace IceCraft.Core.Archive.Repositories;

using System.Collections.Generic;
using IceCraft.Api.Archive.Repositories;

public class DictionaryRepository : Dictionary<string, IPackageSeries>,
    IRepository
{
    public DictionaryRepository()
    {
    }

    public DictionaryRepository(IDictionary<string, IPackageSeries> dictionary) : base(dictionary)
    {
    }

    public DictionaryRepository(IEnumerable<KeyValuePair<string, IPackageSeries>> collection) : base(collection)
    {
    }

    public DictionaryRepository(int capacity) : base(capacity)
    {
    }

    public IEnumerable<IPackageSeries> EnumerateSeries()
    {
        return Values;
    }

    public int GetExpectedSeriesCount()
    {
        return Count;
    }

    public IPackageSeries? GetSeriesOrDefault(string name)
    {
        return this.GetValueOrDefault(name);
    }
}

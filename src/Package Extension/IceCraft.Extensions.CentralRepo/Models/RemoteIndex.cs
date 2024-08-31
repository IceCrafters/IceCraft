namespace IceCraft.Extensions.CentralRepo.Models;

public sealed class RemoteIndex : Dictionary<string, RemoteSeriesEntry>
{
    public RemoteIndex()
    {
    }

    public RemoteIndex(IDictionary<string, RemoteSeriesEntry> dictionary) : base(dictionary)
    {
    }

    public RemoteIndex(IEnumerable<KeyValuePair<string, RemoteSeriesEntry>> collection) : base(collection)
    {
    }

    public RemoteIndex(int capacity) : base(capacity)
    {
    }
}
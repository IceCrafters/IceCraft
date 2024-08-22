namespace IceCraft.Core.Installation.Analysis;

public sealed class DependencyMapBranch : Dictionary<string, DependencyMapEntry>
{
    public DependencyMapBranch()
    {
    }

    public DependencyMapBranch(IDictionary<string, DependencyMapEntry> dictionary) : base(dictionary)
    {
    }

    public DependencyMapBranch(IEnumerable<KeyValuePair<string, DependencyMapEntry>> collection) : base(collection)
    {
    }

    public DependencyMapBranch(int capacity) : base(capacity)
    {
    }
}
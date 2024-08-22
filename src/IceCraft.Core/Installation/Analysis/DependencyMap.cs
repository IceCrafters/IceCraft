namespace IceCraft.Core.Installation.Analysis;

public sealed class DependencyMap : Dictionary<string, DependencyMapBranch>
{
    public DependencyMap()
    {
    }

    public DependencyMap(IDictionary<string, DependencyMapBranch> dictionary) : base(dictionary)
    {
    }

    public DependencyMap(IEnumerable<KeyValuePair<string, DependencyMapBranch>> collection) : base(collection)
    {
    }

    public DependencyMap(int capacity) : base(capacity)
    {
    }
}
namespace IceCraft.Api.Installation.Dependency;

using IceCraft.Api.Package;

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

    public DependencyMapEntry GetEntry(PackageMeta meta)
    {
        if (!TryGetValue(meta.Id, out var branch))
        {
            branch = [];
            Add(meta.Id, branch);
        }

        if (!branch.TryGetValue(meta.Version.ToString(), out var entry))
        {
            entry = new DependencyMapEntry(meta.Id, meta.Version);

            branch.Add(meta.Version.ToString(), entry);
        }

        return entry;
    }
}
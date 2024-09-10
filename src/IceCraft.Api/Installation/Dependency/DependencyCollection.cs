namespace IceCraft.Api.Installation.Dependency;

using System.Collections.ObjectModel;

public class DependencyCollection : Collection<DependencyReference>
{
    public DependencyCollection()
    {
    }

    public DependencyCollection(IList<DependencyReference> list) : base(list)
    {
    }
}
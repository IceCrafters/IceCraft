// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation.Dependency;

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
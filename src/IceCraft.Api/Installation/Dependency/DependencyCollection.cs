// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

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
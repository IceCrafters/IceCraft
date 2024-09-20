// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation.Dependency;

using System.Diagnostics.Contracts;
using IceCraft.Api.Package;

public interface IDependencyMapper
{
    [Pure]
    Task<DependencyMap> MapDependencies();

    [Pure]
    IAsyncEnumerable<DependencyReference> MapUnmetDependencies();

    [Pure]
    IAsyncEnumerable<PackageMeta> EnumerateUnsatisifiedPackages();

    Task<DependencyMap> MapDependenciesCached();
}
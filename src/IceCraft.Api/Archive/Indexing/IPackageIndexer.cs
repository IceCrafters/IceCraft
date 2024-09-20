// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Archive.Indexing;

using IceCraft.Api.Archive.Repositories;

public interface IPackageIndexer
{
    Task<PackageIndex> IndexAsync(IRepositorySourceManager manager, CancellationToken cancellationToken = default);
}

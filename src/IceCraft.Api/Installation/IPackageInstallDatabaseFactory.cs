// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation;

[Obsolete("Use ILocalDatabaseMutator and ILocalDatabaseReadHandle instead.")]
public interface IPackageInstallDatabaseFactory
{
    IPackageInstallDatabase Get();
    
    [Obsolete("Use ILocalDatabaseMutator.StoreAsync() instead.")]
    Task SaveAsync(string filePath);
    [Obsolete("Use ILocalDatabaseMutator.StoreAsync() instead.")]
    Task SaveAsync();
    [Obsolete("Use ILocalDatabaseMutator instead.")]
    Task MaintainAndSaveAsync();
}

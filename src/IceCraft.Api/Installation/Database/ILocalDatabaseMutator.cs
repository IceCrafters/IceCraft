// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation.Database;

using Semver;

public interface ILocalDatabaseMutator : ILocalDatabaseReadHandle
{
    void Remove(string key, SemVersion version);
    
    void Add(InstalledPackageInfo info);
    
    /// <summary>
    /// Adds a new entry or updates an existing entry matching the specified package
    /// metadata.
    /// </summary>
    /// <param name="info">The entry to put.</param>
    void Put(InstalledPackageInfo info);
    
    Task MaintainAsync();
    
    Task StoreAsync();
}
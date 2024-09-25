// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation.Database;

using Semver;

public interface ILocalDatabaseMutator : ILocalDatabaseReadHandle
{
    void Remove(string key, SemVersion version);
    
    void Add(InstalledPackageInfo info);
    
    Task MaintainAsync();
    
    Task StoreAsync();
}
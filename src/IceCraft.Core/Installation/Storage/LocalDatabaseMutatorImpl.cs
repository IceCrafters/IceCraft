// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Installation.Storage;

using IceCraft.Api.Installation;
using IceCraft.Api.Installation.Database;
using Semver;

public class LocalDatabaseMutatorImpl : DatabaseReadHandleImpl, ILocalDatabaseMutator
{
    private readonly DatabaseFile _databaseFile;

    public LocalDatabaseMutatorImpl(DatabaseFile databaseFile) : base(databaseFile)
    {
        _databaseFile = databaseFile;
    }

    public void Remove(string key, SemVersion version)
    {
        _databaseFile.Value.Remove(key, version);
    }

    public void Add(InstalledPackageInfo info)
    {
        _databaseFile.Value.Add(info);
    }

    public void Put(InstalledPackageInfo info)
    {
        _databaseFile.Value.Put(info);
    }

    public async Task MaintainAsync()
    {
        await _databaseFile.Value.MaintainAsync();
    }

    public async Task StoreAsync()
    {
        await _databaseFile.StoreAsync();
    }
}
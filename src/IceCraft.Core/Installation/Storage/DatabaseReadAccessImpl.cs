// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Installation.Storage;

using IceCraft.Api.Client;
using IceCraft.Api.Installation.Database;

public class DatabaseReadAccessImpl : ILocalDatabaseReadAccess
{
    private readonly IFrontendApp _frontend;
    private readonly DatabaseFile _databaseFile;

    public DatabaseReadAccessImpl(IFrontendApp frontend,
        DatabaseFile databaseFile)
    {
        _frontend = frontend;
        _databaseFile = databaseFile;
    }
    
    private PackageInstallDatabaseFactory.ValueMap? _valueMap;
    
    public async Task<ILocalDatabaseReadHandle> GetReadHandle()
    {
        _valueMap ??= await LoadInAsync();

        return new DatabaseReadHandleImpl(_valueMap);
    }

    private async Task<PackageInstallDatabaseFactory.ValueMap> LoadInAsync()
    {
        return await _databaseFile.GetAsync();
    }
}
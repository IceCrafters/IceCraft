// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Installation.Storage;

using IceCraft.Api.Installation;
using IceCraft.Api.Installation.Database;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;

internal class DatabaseReadHandleImpl : ILocalDatabaseReadHandle
{
    private readonly DatabaseObject _database;

    internal DatabaseReadHandleImpl(DatabaseObject database)
    {
        _database = database;
    }

    public InstalledPackageInfo this[string key, string version] => _database[key, version];

    public int Count => _database.Count;
    
    public bool ContainsPackage(string identifier)
    {
        return _database.ContainsKey(identifier);
    }

    public bool ContainsPackage(PackageMeta meta)
    {
        return _database.ContainsMeta(meta);
    }

    public IEnumerable<string> EnumerateKeys()
    {
        return _database.EnumerateKeys();
    }

    public IEnumerable<PackageMeta> EnumeratePackages()
    {
        return _database.EnumeratePackages();
    }

    public InstalledPackageInfo? GetValueOrDefault(PackageMeta meta)
    {
        return _database.GetValueOrDefault(meta);
    }

    public InstalledPackageInfo? GetValueOrDefault(PackageReference reference)
    {
        return _database.GetValueOrDefault(reference);
    }
}
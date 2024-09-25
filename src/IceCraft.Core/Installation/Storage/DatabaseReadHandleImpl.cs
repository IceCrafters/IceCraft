// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Installation.Storage;

using System.Diagnostics.CodeAnalysis;
using IceCraft.Api.Installation;
using IceCraft.Api.Installation.Database;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;
using Semver;

public class DatabaseReadHandleImpl : ILocalDatabaseReadHandle
{
    private readonly DatabaseFile _database;

    internal DatabaseReadHandleImpl(DatabaseFile database)
    {
        _database = database;
    }

    public InstalledPackageInfo this[string key, string version] => _database.Value[key, version];

    public int Count => _database.Value.Count;
    
    public bool ContainsPackage(string identifier)
    {
        return _database.Value.ContainsKey(identifier);
    }

    public bool ContainsPackage(string identifier, string version)
    {
        return _database.Value.TryGetValue(identifier, out var index)
               && index.ContainsKey(version);
    }
    
    public bool ContainsPackage(string identifier, SemVersion version)
    {
        return ContainsPackage(identifier, version.ToString());
    }

    public bool ContainsPackage(PackageMeta meta)
    {
        return _database.Value.ContainsMeta(meta);
    }

    public IEnumerable<string> EnumerateKeys()
    {
        return _database.Value.EnumerateKeys();
    }

    public IEnumerable<PackageMeta> EnumeratePackages()
    {
        return _database.Value.EnumeratePackages();
    }
    
    public IEnumerable<PackageMeta>? EnumeratePackagesOrDefault(string id)
    {
        var index = _database.Value.GetValueOrDefault(id);
        return index?.Values.Select(x => x.Metadata);
    }
    
    public IEnumerable<InstalledPackageInfo> EnumerateEntries(string id)
    {
        return _database.Value[id].Values;
    }

    public InstalledPackageInfo? GetValueOrDefault(PackageMeta meta)
    {
        return _database.Value.GetValueOrDefault(meta);
    }

    public InstalledPackageInfo? GetValueOrDefault(PackageReference reference)
    {
        return _database.Value.GetValueOrDefault(reference);
    }

    public bool TryGetValue(string identifier, SemVersion version, [NotNullWhen(true)] out InstalledPackageInfo? entry)
    {
        entry = null;
        
        if (!_database.Value.TryGetValue(identifier, out var index))
        {
            return false;
        }

        if (!index.TryGetValue(version.ToString(), out var result))
        {
            return false;
        }

        entry = result;
        return true;
    }
}
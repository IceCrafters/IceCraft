// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Installation.Storage;

using IceCraft.Api.Installation;
using IceCraft.Api.Installation.Database;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;
using Semver;

public class LocalDatabaseMutatorImpl : ILocalDatabaseMutator
{
    private readonly DatabaseFile _databaseFile;

    public LocalDatabaseMutatorImpl(DatabaseFile databaseFile)
    {
        _databaseFile = databaseFile;
    }

    public InstalledPackageInfo this[string key, string version] => _databaseFile.Value[key, version];

    public int Count => _databaseFile.Value.Count;
    
    public bool ContainsPackage(string identifier)
    {
        return _databaseFile.Value.ContainsKey(identifier);
    }

    public bool ContainsPackage(PackageMeta meta)
    {
        return _databaseFile.Value.ContainsMeta(meta);
    }

    public IEnumerable<string> EnumerateKeys()
    {
        return _databaseFile.Value.EnumerateKeys();
    }

    public IEnumerable<PackageMeta> EnumeratePackages()
    {
        return _databaseFile.Value.EnumeratePackages();
    }

    public InstalledPackageInfo? GetValueOrDefault(PackageMeta meta)
    {
        return _databaseFile.Value.GetValueOrDefault(meta);
    }

    public InstalledPackageInfo? GetValueOrDefault(PackageReference reference)
    {
        return _databaseFile.Value.GetValueOrDefault(reference);
    }

    public void Remove(string key, SemVersion version)
    {
        _databaseFile.Value.Remove(key, version);
    }

    public void Add(InstalledPackageInfo info)
    {
        _databaseFile.Value.Add(info);
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
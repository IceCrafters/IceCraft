// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Installation.Storage;

using IceCraft.Api.Installation;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;
using Semver;

public sealed class DatabaseObject : Dictionary<string, PackageInstallationIndex>, 
    IPackageInstallDatabase
{
    public InstalledPackageInfo this[string key, string version]
    {
        get => this[key][version];
        set => this[key][version] = value;
    }

    public DatabaseObject()
    {
    }

    public DatabaseObject(IEnumerable<KeyValuePair<string, PackageInstallationIndex>> collection) : base(collection)
    {
    }

    public DatabaseObject(int capacity) : base(capacity)
    {
    }

    public bool ContainsMeta(PackageMeta meta)
    {
        return TryGetValue(meta.Id, out var index)
               && index.TryGetValue(meta.Version.ToString(), out var info)
               && info.Metadata == meta;
    }

    public void Add(InstalledPackageInfo packageInfo)
    {
        Add(packageInfo.Metadata.Id, packageInfo.Metadata.Version, packageInfo);
    }

    public bool TryGetValue(PackageMeta meta, out InstalledPackageInfo? result)
    {
        if (!TryGetValue(meta.Id, out var index))
        {
            result = null;
            return false;
        }

        if (!index.TryGetValue(meta.Version.ToString(), out var verInfo))
        {
            result = null;
            return false;
        }

        if (verInfo.Metadata.Id != meta.Id
            || verInfo.Metadata.Version != meta.Version)
        {
            result = null;
            return false;
        }

        result = verInfo;
        return true;
    }

    public void Put(InstalledPackageInfo info)
    {
        if (!this.TryGetValue(info.Metadata.Id, out var index))
        {
            index = [];
            this.Add(info.Metadata.Id, index);
        }

        index[info.Metadata.Version.ToString()] = info;
    }

    void IPackageInstallDatabase.Remove(string key)
    {
        this.Remove(key);
    }

    public void Add(string key, SemVersion version, InstalledPackageInfo info)
    {
        if (!this.TryGetValue(key, out var index))
        {
            index = [];
            this.Add(key, index);
        }

        index.Add(version.ToString(), info);
    }

    public void Remove(string key, SemVersion version)
    {
        if (!this.TryGetValue(key, out var index))
        {
            return;
        }

        index.Remove(version.ToString());
    }

    public async Task MaintainAsync()
    {
        var keysToDelete = new List<string>(Count / 2);

        await Task.Run(() =>
        {
            foreach (var index in this)
            {
                if (index.Value.Count == 0)
                {
                    keysToDelete.Add(index.Key);
                }
            }
        });

        await Task.Run(() =>
        {
            foreach (var key in keysToDelete)
            {
                Remove(key);
            }
        });
    }

    public IEnumerable<string> EnumerateKeys()
    {
        return Keys;
    }

    public IEnumerable<PackageMeta> EnumeratePackages()
    {
        return this.SelectMany(index => index.Value)
            .Select(package => package.Value.Metadata)
            .AsParallel();
    }

    public PackageInstallationIndex? GetValueOrDefault(string packageId)
    {
        return CollectionExtensions.GetValueOrDefault(this, packageId);
    }

    public InstalledPackageInfo? GetValueOrDefault(PackageReference reference)
    {
        var index = GetValueOrDefault(reference.PackageId);

        return index?.GetValueOrDefault(reference.PackageVersion.ToString());
    }

    public InstalledPackageInfo? GetValueOrDefault(PackageMeta meta)
    {
        var index = GetValueOrDefault(meta.Id);

        return index?.GetValueOrDefault(meta.Version.ToString());
    }
}
// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Installation.Storage;

using System;
using System.Text.Json;
using IceCraft.Api.Client;
using IceCraft.Api.Installation;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;
using IceCraft.Core.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Semver;
using InstalledPackageMap = Dictionary<string, Api.Installation.PackageInstallationIndex>;

public class PackageInstallDatabaseFactory : IPackageInstallDatabaseFactory
{
    private readonly IFrontendApp _frontend;
    private readonly ServiceProvider _serviceProvider;
    private readonly string _databasePath;

    private ValueMap? _map;

    public PackageInstallDatabaseFactory(IFrontendApp frontend,
        ServiceProvider serviceProvider)
    {
        _frontend = frontend;

        var packagesPath = Path.Combine(_frontend.DataBasePath, PackageInstallManager.PackagePath);
        _databasePath = Path.Combine(packagesPath, "db.json");

        Directory.CreateDirectory(packagesPath);

        _serviceProvider = serviceProvider;
    }

    public async Task<IPackageInstallDatabase> GetAsync()
    {
        if (_map != null) return _map;
        
        _frontend.Output.Log("Loading package database...");

        var packagesPath = Path.Combine(_frontend.DataBasePath, PackageInstallManager.PackagePath);

        await using var scope = _serviceProvider.CreateAsyncScope();
        var loader = scope.ServiceProvider.GetRequiredService<DatabaseFile>();
        var retVal = await loader.LoadDatabaseAsync(Path.Combine(packagesPath, "db.json"));
        _map = retVal;
        return retVal;
    }

    public async Task SaveAsync()
    {
        await SaveAsync(_databasePath);
    }

    public async Task MaintainAndSaveAsync()
    {
        _ = await GetAsync();
        var showHint = false;
        var orphanedVirtual = new List<PackageMeta>();
        foreach (var (_, packageInfo) in 
                 _map!.SelectMany(x => x.Value))
        {
            // ReSharper disable once InvertIf
            if (packageInfo.State == InstallationState.Virtual)
            {
                if (!packageInfo.ProvidedBy.HasValue)
                {
                    _frontend.Output.Warning("Virtual package {0} ({1}) is provided by nobody", 
                        packageInfo.Metadata.Id,
                        packageInfo.Metadata.Version);
                    showHint = true;
                }
                else
                {
                    var info = _map!.GetValueOrDefault(packageInfo.ProvidedBy.Value);
                    if (info == null)
                    {
                        orphanedVirtual.Add(packageInfo.Metadata);
                    }
                }
            }
        }

        foreach (var package in orphanedVirtual)
        {
            if (_map == null)
            {
                return;
            }

            _map[package.Id].Remove(package.Version.ToString());
        }
        
        if (showHint)
        {
            _frontend.Output.Log("HINT: Use 'IceCraft auto-remove' to remove orphaned virtual packages.");
        }

        await SaveAsync();
    }

    public async Task SaveAsync(string filePath)
    {
        _frontend.Output.Verbose("Saving database");

        if (_map == null)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var loader = scope.ServiceProvider.GetRequiredService<DatabaseFile>();
            
            await loader.LoadDatabaseAsync(filePath);
            return;
        }

        try
        {
            await using var fileStream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(fileStream, _map, IceCraftCoreContext.Default.PackageInstallValueMap);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to save installation database.", ex);
        }
    }

    internal class ValueMap : InstalledPackageMap, IPackageInstallDatabase
    {
        public InstalledPackageInfo this[string key, string version]
        {
            get => this[key][version];
            set => this[key][version] = value;
        }

        public ValueMap()
        {
        }

        public ValueMap(IEnumerable<KeyValuePair<string, PackageInstallationIndex>> collection) : base(collection)
        {
        }

        public ValueMap(int capacity) : base(capacity)
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
}

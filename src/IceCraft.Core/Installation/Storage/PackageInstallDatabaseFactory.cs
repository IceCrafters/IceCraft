// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Installation.Storage;

using System;
using System.Text.Json;
using IceCraft.Api.Client;
using IceCraft.Api.Installation;
using IceCraft.Api.Package;
using IceCraft.Core.Serialization;
using Microsoft.Extensions.DependencyInjection;

public class PackageInstallDatabaseFactory : IPackageInstallDatabaseFactory
{
    private readonly IFrontendApp _frontend;
    private readonly ServiceProvider _serviceProvider;
    private readonly string _databasePath;

    private DatabaseObject? _map;

    public PackageInstallDatabaseFactory(IFrontendApp frontend,
        ServiceProvider serviceProvider)
    {
        _frontend = frontend;

        var packagesPath = Path.Combine(_frontend.DataBasePath, PackageInstallManager.PackagePath);
        _databasePath = Path.Combine(packagesPath, "db.json");

        Directory.CreateDirectory(packagesPath);

        _serviceProvider = serviceProvider;
    }

    public IPackageInstallDatabase Get()
    {
        if (_map != null) return _map;
        
        _frontend.Output.Log("Loading package database...");

        var packagesPath = Path.Combine(_frontend.DataBasePath, PackageInstallManager.PackagePath);
        
        var loader = _serviceProvider.GetRequiredService<DatabaseFile>();
        return loader.Get();
    }

    public async Task SaveAsync()
    {
        await SaveAsync(_databasePath);
    }

    public async Task MaintainAndSaveAsync()
    {
        _ = Get();
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
            _map = _serviceProvider.GetRequiredService<DatabaseFile>().Get();
            return;
        }

        try
        {
            await using var fileStream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(fileStream, _map, IceCraftCoreContext.Default.DatabaseObject);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to save installation database.", ex);
        }
    }
}

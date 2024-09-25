// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Installation.Storage;

using System.Text.Json;
using IceCraft.Api.Client;
using IceCraft.Core.Serialization;

public sealed class DatabaseFile
{
    private readonly IFrontendApp _frontend;
    private PackageInstallDatabaseFactory.ValueMap? _database; 

    public DatabaseFile(IFrontendApp frontend)
    {
        _frontend = frontend;
    }

    internal async Task<PackageInstallDatabaseFactory.ValueMap> GetAsync()
    {
        if (_database != null)
        {
            return _database;
        }
        
        var packagesPath = Path.Combine(_frontend.DataBasePath, PackageInstallManager.PackagePath);
        var retVal = await LoadDatabaseAsync(Path.Combine(packagesPath, "db.json"));
        _database = retVal;
        return retVal;
    }
    
    private static async Task<PackageInstallDatabaseFactory.ValueMap> CreateDatabaseFileAsync(string filePath)
    {
        var retVal = new PackageInstallDatabaseFactory.ValueMap();
        try
        {
            await using var fileStream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(fileStream, retVal, IceCraftCoreContext.Default.PackageInstallValueMap);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to create installation database.", ex);
        }

        return retVal;
    }
    
    internal async Task<PackageInstallDatabaseFactory.ValueMap> LoadDatabaseAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return await CreateDatabaseFileAsync(filePath);
        }

        // If database file exists, load existing file
        PackageInstallDatabaseFactory.ValueMap? retVal;
        try
        {
            await using var fileStream = File.OpenRead(filePath);
            retVal = await JsonSerializer.DeserializeAsync(fileStream,
                IceCraftCoreContext.Default.PackageInstallValueMap);
        }
        catch (JsonException ex)
        {
            _frontend.Output.Warning(ex, "Json format failure");
            return await CreateDatabaseFileAsync(filePath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load installation database.", ex);
        }

        if (retVal == null)
        {
            File.Delete(filePath);
            return await CreateDatabaseFileAsync(filePath);
        }

        _frontend.Output.Verbose("{0} packages currently installed", retVal.Count);

        return retVal;
    }
}
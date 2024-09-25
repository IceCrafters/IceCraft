// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Installation.Storage;

using System.Text.Json;
using IceCraft.Api.Client;
using IceCraft.Core.Serialization;

public sealed class DatabaseFile
{
    private readonly DatabaseObject _database;
    private readonly IOutputAdapter _output;

    public DatabaseFile(DatabaseObject database,
        IOutputAdapter output)
    {
        _database = database;
        _output = output;
    }

    public DatabaseObject Get()
    {
        return _database;
    }
    
    private static async Task<DatabaseObject> CreateDatabaseFileAsync(string filePath)
    {
        var retVal = new DatabaseObject();
        try
        {
            await using var fileStream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(fileStream, retVal, IceCraftCoreContext.Default.DatabaseObject);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to create installation database.", ex);
        }

        return retVal;
    }
    
    public static async Task<DatabaseObject> LoadDatabaseAsync(string filePath,
        IOutputAdapter? outputAdapter = null)
    {
        if (!File.Exists(filePath))
        {
            return await CreateDatabaseFileAsync(filePath);
        }

        // If database file exists, load existing file
        DatabaseObject? retVal;
        try
        {
            await using var fileStream = File.OpenRead(filePath);
            retVal = await JsonSerializer.DeserializeAsync(fileStream,
                IceCraftCoreContext.Default.DatabaseObject);
        }
        catch (JsonException ex)
        {
            outputAdapter?.Warning(ex, "Json format failure");
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

        outputAdapter?.Verbose("{0} packages currently installed", retVal.Count);

        return retVal;
    }
}
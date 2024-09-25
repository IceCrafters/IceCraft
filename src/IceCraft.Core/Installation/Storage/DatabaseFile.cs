// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Installation.Storage;

using System.Text.Json;
using IceCraft.Api.Client;
using IceCraft.Core.Serialization;

public sealed class DatabaseFile
{
    private readonly IOutputAdapter _output;
    private readonly string _writeTo;

    public DatabaseFile(DatabaseObject database,
        IOutputAdapter output,
        string writeTo)
    {
        Value = database;
        _output = output;
        _writeTo = writeTo;
    }

    public DatabaseObject Value { get; }

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

    public async Task StoreAsync()
    {
        _output.Verbose("Saving database");

        try
        {
            await using var fileStream = File.Create(_writeTo);
            await JsonSerializer.SerializeAsync(fileStream, Value, IceCraftCoreContext.Default.DatabaseObject);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to save installation database.", ex);
        }
    }
}
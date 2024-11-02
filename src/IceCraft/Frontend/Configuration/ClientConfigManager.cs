// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Frontend.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using IceCraft.Api.Client;
using IceCraft.Api.Configuration;

public class ClientConfigManager : IConfigManager
{
    private static readonly string GlobalConfigHome = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "IceCraft.d");

    private readonly string _configDirectory;

    static ClientConfigManager()
    {
        Directory.CreateDirectory(GetEffectiveConfigHome());
    }

    private const string ConfigDirectoryName = "config";

    public ClientConfigManager(IFrontendApp frontend)
    {
        _configDirectory = Path.Combine(frontend.DataBasePath, ConfigDirectoryName);
        Directory.CreateDirectory(_configDirectory);
    }

    public T? GetJsonConfigFile<T>(string fileName, JsonTypeInfo<T> typeInfo, T? defaultValue = default)
    {
        Util.CheckFileName(fileName);
        var path = Path.Combine(_configDirectory, $"{fileName}.json");

        if (!File.Exists(path))
        {
            return defaultValue;
        }

        using var stream = File.OpenRead(path);

        return JsonSerializer.Deserialize(stream, typeInfo);
    }

    public T? GetJsonConfigFile<T>(string fileName, T? defaultValue = default)
    {
        Util.CheckFileName(fileName);
        var path = Path.Combine(_configDirectory, $"{fileName}.json");

        if (!File.Exists(path))
        {
            return defaultValue;
        }

        using var stream = File.OpenRead(path);

        return JsonSerializer.Deserialize<T>(stream);
    }

    public async Task<T?> GetJsonConfigFileAsync<T>(string fileName, JsonTypeInfo<T> typeInfo, T? defaultValue = default)
    {
        Util.CheckFileName(fileName);
        var path = Path.Combine(_configDirectory, $"{fileName}.json");

        if (!File.Exists(path))
        {
            return defaultValue;
        }

        await using var stream = File.OpenRead(path);

        return await JsonSerializer.DeserializeAsync(stream, typeInfo);
    }

    public async Task<T?> GetJsonConfigFileAsync<T>(string fileName, T? defaultValue = default)
    {
        Util.CheckFileName(fileName);
        var path = Path.Combine(_configDirectory, $"{fileName}.json");

        if (!File.Exists(path))
        {
            return defaultValue;
        }

        await using var stream = File.OpenRead(path);

        return await JsonSerializer.DeserializeAsync<T>(stream);
    }

    public async Task WriteJsonConfigFileAsync<T>(string fileName, JsonTypeInfo<T> typeInfo, T value)
    {
        Util.CheckFileName(fileName);
        var path = Path.Combine(_configDirectory, $"{fileName}.json");

        await using var stream = File.Create(path);

        await JsonSerializer.SerializeAsync(stream, value, typeInfo);
    }

    public async Task WriteJsonConfigFileAsync<T>(string fileName, T value)
    {
        Util.CheckFileName(fileName);
        var path = Path.Combine(_configDirectory, $"{fileName}.json");

        await using var stream = File.Create(path);

        await JsonSerializer.SerializeAsync(stream, value);
    }
    
    internal static string GetEffectiveConfigHome()
    {
        return Environment.GetEnvironmentVariable("ICECRAFT_CONFIG_HOME") ?? GlobalConfigHome;
    }
}

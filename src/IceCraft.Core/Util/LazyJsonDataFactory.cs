namespace IceCraft.Core.Util;

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

public class LazyJsonDataFactory<T>
{
    private readonly JsonTypeInfo<T> _typeInfo;
    private readonly string _filePath;
    private readonly Func<T> _newFunc;
    private T? _instance;

    public LazyJsonDataFactory(string filePath, JsonTypeInfo<T> typeInfo, Func<T> provider)
    {
        _filePath = filePath;
        _typeInfo = typeInfo;
        _newFunc = provider;
    }

    public async Task<T> GetAsync()
    {
        if (_instance != null)
        {
            return _instance;
        }

        if (!File.Exists(_filePath))
        {
            return await CreateFile();
        }

        T? result;
        try
        {
            result = await LoadFile();
        }
        catch
        {
            return await CreateFile();
        }

        if (result == null)
        {
            return await CreateFile();
        }

        _instance = result;
        return result;
    }

    public async Task SaveFile()
    {
        if (_instance == null)
        {
            throw new InvalidOperationException("Data file not loaded yet.");
        }

        using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, _instance, _typeInfo);
    }

    private async Task<T> CreateFile()
    {
        var result = _newFunc();
        using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, result, _typeInfo);
        return result;
    }

    private async Task<T?> LoadFile()
    {
        using var stream = File.OpenRead(_filePath);
        return await JsonSerializer.DeserializeAsync(stream, _typeInfo);
    }
}

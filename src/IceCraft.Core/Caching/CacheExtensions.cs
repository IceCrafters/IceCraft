namespace IceCraft.Core.Caching;

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using JetBrains.Annotations;

[PublicAPI]
public static class CacheExtensions
{
    public static T RollJson<T>(this ICacheStorage storage, string objId, Func<T> defaultSupplier, JsonSerializerOptions? options = null, bool reset = false)
    {
        if (!reset && storage.DoesObjectExist(objId))
        {
            try
            {
                using var readStream = storage.OpenReadObject(objId);
                var retVal = storage.ReadJson<T>(objId, options);

                if (retVal != null)
                {
                    return retVal;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("WARNING: Failed to read object '{0}'", objId);
                Console.WriteLine(ex);
            }
        }

        var supplied = defaultSupplier();

        try
        {
            storage.CreateJson(objId, supplied, options);
        }
        catch (Exception ex)
        {
            Console.WriteLine("WARNING: Failed to store object '{0}'", objId);
            Console.WriteLine(ex);
        }

        return supplied;
    }

    public static async Task<T> RollJsonAsync<T>(this ICacheStorage storage, string objId, Func<Task<T>> defaultSupplier, JsonSerializerOptions? options = null, bool reset = false)
    {
        if (!reset && storage.DoesObjectExist(objId))
        {
            try
            {
                await using var readStream = storage.OpenReadObject(objId);
                var retVal = await storage.ReadJsonAsync<T>(objId, options);

                if (retVal != null)
                {
                    return retVal;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("WARNING: Failed to read object '{0}'", objId);
                Console.WriteLine(ex);
            }
        }

        var supplied = await defaultSupplier();

        try
        {
            await storage.CreateJsonAsync(objId, supplied, options);
        }
        catch (Exception ex)
        {
            Console.WriteLine("WARNING: Failed to store object '{0}'", objId);
            Console.WriteLine(ex);
        }

        return supplied;
    }

    public static async Task<T> RollJsonAsync<T>(this ICacheStorage storage, 
        string objId, 
        Func<Task<T>> defaultSupplier, 
        JsonTypeInfo<T> typeInfo,
        bool reset = false)
    {
        if (!reset && storage.DoesObjectExist(objId))
        {
            try
            {
                await using var readStream = storage.OpenReadObject(objId);
                var retVal = await storage.ReadJsonAsync(objId, typeInfo);

                if (retVal != null)
                {
                    return retVal;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("WARNING: Failed to read object '{0}'", objId);
                Console.WriteLine(ex);
            }
        }

        var supplied = await defaultSupplier();

        try
        {
            await storage.CreateJsonAsync(objId, supplied, typeInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine("WARNING: Failed to store object '{0}'", objId);
            Console.WriteLine(ex);
        }

        return supplied;
    }

    public static T? ReadJson<T>(this ICacheStorage storage, string objId, JsonSerializerOptions? options = null)
    {
        using var stream = storage.OpenReadObject(objId);
        return JsonSerializer.Deserialize<T>(stream, options);
    }

    public static async Task<T?> ReadJsonAsync<T>(this ICacheStorage storage, string objId, JsonSerializerOptions? options = null)
    {
        await using var stream = storage.OpenReadObject(objId);
        return await JsonSerializer.DeserializeAsync<T>(stream, options);
    }

    public static async Task<T?> ReadJsonAsync<T>(this ICacheStorage storage, 
        string objId, 
        JsonTypeInfo<T> typeInfo)
    {
        await using var stream = storage.OpenReadObject(objId);
        return await JsonSerializer.DeserializeAsync(stream, typeInfo);
    }

    public static void CreateJson<T>(this ICacheStorage storage, string objId, T value, JsonSerializerOptions? options = null)
    {
        using var stream = storage.CreateObject(objId);
        JsonSerializer.Serialize(stream, value, options);
    }

    public static async Task CreateJsonAsync<T>(this ICacheStorage storage, string objId, T value, JsonSerializerOptions? options = null)
    {
        await using var stream = storage.CreateObject(objId);
        await JsonSerializer.SerializeAsync(stream, value, options);
    }

    public static async Task CreateJsonAsync<T>(this ICacheStorage storage, 
        string objId, 
        T value, 
        JsonTypeInfo<T> typeInfo)
    {
        await using var stream = storage.CreateObject(objId);
        await JsonSerializer.SerializeAsync(stream, value, typeInfo);
    }
}

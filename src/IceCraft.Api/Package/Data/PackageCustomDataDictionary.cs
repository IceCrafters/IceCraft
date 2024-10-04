// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Package.Data;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

public class PackageCustomDataDictionary : Dictionary<string, JsonElement>
{
    public PackageCustomDataDictionary()
    {
    }

    public PackageCustomDataDictionary(IDictionary<string, JsonElement> dictionary) : base(dictionary)
    {
    }

    public PackageCustomDataDictionary(IEnumerable<KeyValuePair<string, JsonElement>> collection) : base(collection)
    {
    }

    public PackageCustomDataDictionary(int capacity) : base(capacity)
    {
    }

    /// <summary>
    /// Serializes the supplied value and adds the key and the serialized value to the
    /// dictionary.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <param name="typeInfo">The type info to use with serialization.</param>
    /// <typeparam name="T">The type of the value.</typeparam>
    public void AddSerialize<T>(string key, T? value, JsonTypeInfo<T?> typeInfo)
    {
        var element = JsonSerializer.SerializeToElement(value, typeInfo);
        
        Add(key, element);
    }
    
    /// <summary>
    /// Serializes the supplied value and replaces the value associated with the key
    /// with serialized value. If the key does not exist, this method behaves like
    /// <see cref="AddSerialize{T}"/>.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <param name="typeInfo">The type info to use with serialization.</param>
    /// <typeparam name="T">The type of the value.</typeparam>
    public void PutSerialize<T>(string key, T? value, JsonTypeInfo<T?> typeInfo)
    {
        var element = JsonSerializer.SerializeToElement(value, typeInfo);
        
        this[key] = element;
    }

    public T? GetDeserialize<T>(string key, JsonTypeInfo<T> typeInfo)
    {
        var data = this[key];

        return data.Deserialize(typeInfo);
    }
    
    public bool TryGetValueDeserialize<T>(string key, JsonTypeInfo<T> typeInfo, [NotNullWhen(true)] out T? result)
    {
        if (!this.TryGetValue(key, out var element))
        {
            result = default;
            return false;
        }
        
        result = element.Deserialize(typeInfo);
        if (result == null)
        {
            return false;
        }

        return true;
    }
}
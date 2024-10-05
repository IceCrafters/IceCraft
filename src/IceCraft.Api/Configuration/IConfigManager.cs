// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Configuration;
using System.Text.Json.Serialization.Metadata;

public interface IConfigManager
{
    T? GetJsonConfigFile<T>(string fileName, JsonTypeInfo<T> typeInfo,  T? defaultValue = default);
    T? GetJsonConfigFile<T>(string fileName, T? defaultValue = default);

    Task<T?> GetJsonConfigFileAsync<T>(string fileName, JsonTypeInfo<T> typeInfo,  T? defaultValue = default);
    Task<T?> GetJsonConfigFileAsync<T>(string fileName, T? defaultValue = default);

    Task WriteJsonConfigFileAsync<T>(string fileName, JsonTypeInfo<T> typeInfo, T value);
    Task WriteJsonConfigFileAsync<T>(string fileName, T value);
}

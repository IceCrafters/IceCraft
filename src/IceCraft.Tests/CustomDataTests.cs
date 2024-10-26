// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Tests;

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using IceCraft.Api.Package.Data;

public class CustomDataTests
{
    private record struct SampleData(int Value);

    [Fact]
    public void CustomDataDictionary_Serialized_ValueType()
    {
        // Arrange
        const string key = "test-key";
        var data = new SampleData(10);
        var typeInfo = JsonTypeInfo.CreateJsonTypeInfo<SampleData>(JsonSerializerOptions.Default);
        
        var dict = new PackageCustomDataDictionary();
        dict.PutSerialize(key, data, typeInfo);
        
        // Act
        var success = dict.TryGetValueDeserialize(key, typeInfo, out var value);
        
        // Assert
        Assert.True(success);
        Assert.Equal(data, value);
    }
}
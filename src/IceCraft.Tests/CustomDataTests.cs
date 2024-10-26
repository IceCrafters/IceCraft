// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Tests;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package.Data;
using IceCraft.Api.Serialization;
using Newtonsoft.Json;
using Semver;

public class CustomDataTests
{
    [Fact]
    public void CustomDataDictionary_Serialized_ValueType()
    {
        // Arrange
        const string key = "test-key";
        var data = new DependencyReference("id", SemVersionRange.All);
        
        var dict = new PackageCustomDataDictionary();
        dict.PutSerialize(key, data, IceCraftApiContext.Default.DependencyReference);
        
        // Act
        var success = dict.TryGetValueDeserialize(key, IceCraftApiContext.Default.DependencyReference, out var value);
        
        // Assert
        Assert.True(success);
        Assert.Equal(data, value);
    }
}
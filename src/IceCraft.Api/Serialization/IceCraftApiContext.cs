// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Serialization;

using System.Text.Json.Serialization;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;
using IceCraft.Api.Package.Data;

[JsonSerializable(typeof(DependencyCollection))]
[JsonSerializable(typeof(PackageMeta))]
[JsonSerializable(typeof(DependencyReference))]
[JsonSerializable(typeof(PackageAuthorInfo))]
[JsonSerializable(typeof(PackageTranscript))]
[JsonSerializable(typeof(PackagePluginInfo))]
[JsonSerializable(typeof(PackageCustomDataDictionary))]
public sealed partial class IceCraftApiContext : JsonSerializerContext 
{
}
// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Frontend.Configuration;

using System.Text.Json;
using System.Text.Json.Serialization;

[JsonSerializable(typeof(ClientConfigModel))]
[JsonSourceGenerationOptions(WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    ReadCommentHandling = JsonCommentHandling.Skip)]
internal partial class ClientConfigContext : JsonSerializerContext;
// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Util;

using System.Text.Json.Serialization;

[JsonSerializable(typeof(RepoConfigFactory.RepoConfigData))]
public partial class CsrPluginJsonContext : JsonSerializerContext
{
}

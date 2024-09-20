// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Archive.Indexing;

using System.Text.Json.Serialization;
using IceCraft.Api.Serialization;
using Semver;

public sealed record CachedPackageSeriesInfo
{
    public required string Name { get; init; }
    public required IDictionary<string, CachedPackageInfo> Versions { get; init; }

    [JsonConverter(typeof(SemVersionConverter))]
    [Obsolete("This property is deprecated and CachedIndexer no longer fills it. Compare semvers instead.")]
    public SemVersion? LatestVersion { get; init; }
}

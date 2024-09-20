// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Archive.Indexing;

using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Package;

public record CachedPackageInfo
{
    public required PackageMeta Metadata { get; init; }
    public required RemoteArtefact Artefact { get; init; }
    public IEnumerable<ArtefactMirrorInfo>? Mirrors { get; init; }
    public string? BestMirror { get; init; }
}

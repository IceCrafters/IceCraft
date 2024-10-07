// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Network;

using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Package;

public record RemotePackageInfo
{
    public required PackageMeta Metadata { get; init; }
    public required IArtefactDefinition Artefact { get; init; }
    public required IList<ArtefactMirrorInfo> Mirrors { get; init; }
}
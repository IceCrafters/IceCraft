// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Archive.Repositories;

using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Package;

public interface IPackage
{
    public IPackageSeries Series { get; }

    public RemoteArtefact GetArtefact();

    PackageMeta GetMeta();

    IEnumerable<ArtefactMirrorInfo>? GetMirrors();
}

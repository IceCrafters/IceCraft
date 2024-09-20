// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Impl;

using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Package;
using IceCraft.Extensions.CentralRepo.Network;

public class RemotePackage : IPackage
{
    private readonly RemotePackageInfo _remotePackageInfo;

    public RemotePackage(RemotePackageInfo remotePackageInfo, RemotePackageSeries packageSeries)
    {
        Series = packageSeries;
        _remotePackageInfo = remotePackageInfo;
    }

    public IPackageSeries Series { get; }

    public RemoteArtefact GetArtefact()
    {
        return _remotePackageInfo.Artefact;
    }

    public PackageMeta GetMeta()
    {
        return _remotePackageInfo.Metadata;
    }

    public IEnumerable<ArtefactMirrorInfo> GetMirrors()
    {
        return _remotePackageInfo.Mirrors;
    }
}
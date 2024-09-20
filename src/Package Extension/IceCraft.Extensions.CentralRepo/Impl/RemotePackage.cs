namespace IceCraft.Extensions.CentralRepo.Impl;

using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Package;
using IceCraft.Core.Archive;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Extensions.CentralRepo.Models;
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
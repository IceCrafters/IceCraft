namespace IceCraft.Extensions.CentralRepo.Impl;

using IceCraft.Core.Archive;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Extensions.CentralRepo.Models;

public class RemotePackage : IPackage
{
    private readonly RemoteVersionEntry _versionEntry;
    private readonly string _id;
    private readonly RemotePackageSeries _series;

    private static readonly PackagePluginInfo PluginInfo = new PackagePluginInfo()
    {
        ConfiguratorRef = "ic_csr",
        InstallerRef = "ic_csr",
        PreProcessorRef = "ic_csr"
    };

    public RemotePackage(RemoteVersionEntry versionEntry, RemotePackageSeries packageSeries, string id)
    {
        _versionEntry = versionEntry;
        _series = packageSeries;
        _id = id;
    }

    public IPackageSeries Series => _series;

    public RemoteArtefact GetArtefact()
    {
        return _versionEntry.Artefact;
    }

    public PackageMeta GetMeta()
    {
        return new PackageMeta
        {
            Version = _versionEntry.Version,
            Id = _id,
            PluginInfo = PluginInfo,
            ReleaseDate = _versionEntry.ReleaseDate,
            Transcript = _versionEntry.Transcript ?? _series.Transcript,
            Unitary = _versionEntry.Unitary,
            Dependencies = _versionEntry.Dependencies,
            ConflictsWith = _versionEntry.ConflictsWith
        };
    }

    public IEnumerable<ArtefactMirrorInfo>? GetMirrors()
    {
        return _versionEntry.Mirrors;
    }
}
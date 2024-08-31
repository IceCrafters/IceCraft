namespace IceCraft.Extensions.CentralRepo.Impl;

using IceCraft.Core.Archive;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Extensions.CentralRepo.Models;

public class RemotePackage : IPackage
{
    private readonly RemoteVersionEntry _versionEntry;
    private readonly string _id;

    private static readonly PackagePluginInfo PluginInfo = new PackagePluginInfo()
    {
        ConfiguratorRef = "ic_csr",
        InstallerRef = "ic_csr",
        PreProcessorRef = "ic_csr"
    };

    public RemotePackage(RemoteVersionEntry versionEntry, IPackageSeries packageSeries, string id)
    {
        _versionEntry = versionEntry;
        Series = packageSeries;
        _id = id;
    }

    public IPackageSeries Series { get; }

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
            Transcript = _versionEntry.Transcript,
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
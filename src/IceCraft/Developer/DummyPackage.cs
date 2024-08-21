namespace IceCraft.Developer;

using System.Collections.Generic;
using IceCraft.Core.Archive;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Dependency;
using IceCraft.Core.Archive.Packaging;
using Semver;

public class DummyPackage : IPackage
{
    private readonly DummyPackageSeries _series;
    private readonly SemVersion _version;
    private readonly DependencyCollection? _dependencyCollection;
    private readonly string _name;

    public DummyPackage(string name, DummyPackageSeries series, SemVersion version, DependencyCollection? dependencyCollection)
    {
        _name = name;
        _series = series;
        _version = version;
        _dependencyCollection = dependencyCollection;
    }

    public IPackageSeries Series => _series;

    public RemoteArtefact GetArtefact()
    {
        return new RemoteArtefact
        {
            ChecksumType = "sha256",
            Checksum = "5e9a7996fe94d7be10595d7133748760bf8348198b71b7a50fd8affaa980ac61"
        };
    }

    public PackageMeta GetMeta()
    {
        return new PackageMeta()
        {
            Id = _name,
            PluginInfo = new PackagePluginInfo("dummy-test", "dummy-test"),
            ReleaseDate = DateTime.MinValue,
            Version = _version,
            Dependencies = _dependencyCollection
        };
    }

    public IEnumerable<ArtefactMirrorInfo>? GetMirrors()
    {
        return
        [
            new ArtefactMirrorInfo
            {
                DownloadUri = new Uri("http://www.msftconnecttest.com/connecttest.txt"),
                Name = "main",
                IsOrigin = true
            }
        ];
    }
}
namespace IceCraft.Developer;

using System.Collections.Generic;
using IceCraft.Core.Archive;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Packaging;

public class DummyPackage : IPackage
{
    private readonly DummyPackageSeries _series;

    public DummyPackage(DummyPackageSeries series)
    {
        _series = series;
    }

    public IPackageSeries Series => _series;

    public RemoteArtefact GetArtefact()
    {
        return new RemoteArtefact()
        {
            DownloadUri = new Uri("http://www.msftconnecttest.com/connecttest.txt"),
            ChecksumType = "sha256",
            Checksum = "5e9a7996fe94d7be10595d7133748760bf8348198b71b7a50fd8affaa980ac61"
        };
    }

    public PackageMeta GetMeta()
    {
        return new PackageMeta()
        {
            Id = "dummy-test",
            PluginInfo = new PackagePluginInfo("dummy-test", "dummy-test"),
            ReleaseDate = DateTime.MinValue,
            Version = "0.1.0+dummy"
        };
    }

    public IEnumerable<ArtefactMirrorInfo>? GetMirrors()
    {
        return [new ArtefactMirrorInfo()
        {
            DownloadUri = new Uri("http://www.msftconnecttest.com/connecttest.txt"),
            ChecksumType = "sha256",
            Checksum = "5e9a7996fe94d7be10595d7133748760bf8348198b71b7a50fd8affaa980ac61",
            Name = "main",
            IsOrigin = true
        }];
    }
}

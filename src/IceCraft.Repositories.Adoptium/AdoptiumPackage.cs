namespace IceCraft.Repositories.Adoptium;

using IceCraft.Core.Archive;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Checksums;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Repositories.Adoptium.Models;

public class AdoptiumPackage : IPackage
{
    private readonly AdoptiumPackageSeries _series;
    private readonly AdoptiumBinaryAssetView _asset;

    internal AdoptiumPackage(AdoptiumPackageSeries series, AdoptiumBinaryAssetView asset)
    {
        _series = series;
        _asset = asset;
    }

    public IPackageSeries Series => _series;

    public RemoteArtefact GetArtefact()
    {
        return new RemoteArtefact(_asset.Binary!.Package!.Link,
            _asset.Binary!.Package!.Checksum,
            Sha256ChecksumValidator.Shared);
    }

    public PackageMeta GetMeta()
    {
        var asset = _asset;

        return new PackageMeta(_series.Name,
            version: (asset.Version?.Semver) ?? (asset.ReleaseName),
            releaseDate: asset.Binary?.UpdatedAt ?? DateTime.MinValue);
    }
}

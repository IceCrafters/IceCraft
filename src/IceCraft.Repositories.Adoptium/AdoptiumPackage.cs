namespace IceCraft.Repositories.Adoptium;

using IceCraft.Core.Archive;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Checksums;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Repositories.Adoptium.Models;
using Microsoft.Extensions.Logging;

public class AdoptiumPackage : IPackage
{
    private readonly AdoptiumPackageSeries _series;
    private readonly AdoptiumBinaryAssetView _asset;
    private readonly ILogger _logger;

    internal AdoptiumPackage(AdoptiumPackageSeries series, AdoptiumBinaryAssetView asset, ILogger logger)
    {
        _series = series;
        _asset = asset;
        _logger = logger;
    }

    public IPackageSeries Series => _series;

    public RemoteArtefact GetArtefact()
    {
        var binary = (_asset.Binaries?.FirstOrDefault())
         ?? throw new NotSupportedException("Asset does not contain binary asset");
        return new RemoteArtefact
        {
            DownloadUri = binary.Package!.Link,
            Checksum = binary.Package!.Checksum,
            ChecksumType = "sha256"
        };
    }

    public PackageMeta GetMeta()
    {
        var asset = _asset;

        return new PackageMeta(_series.Name,
            version: (asset.VersionData?.Semver) ?? (asset.ReleaseName),
            releaseDate: asset.Timestamp ?? asset.UpdatedAt ?? DateTime.MinValue);
    }
}

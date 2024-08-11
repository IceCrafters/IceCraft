namespace IceCraft.Repositories.Adoptium;

using System.Collections.Generic;
using System.Runtime.InteropServices;
using Flurl;
using IceCraft.Core.Archive;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Checksums;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Repositories.Adoptium.Models;
using Microsoft.Extensions.Logging;

public class AdoptiumPackage : IPackage
{
    private readonly AdoptiumPackageSeries _series;
    private readonly AdoptiumBinaryRelease _asset;
    private readonly ILogger _logger;

    private string? GetTunaMirrorUrl()
    {
        const string TunaAdoptiumRoot = "https://mirrors.tuna.tsinghua.edu.cn/Adoptium";

        if (_asset.VersionData == null)
        {
            return null;
        }

        var artefact = _asset.Binaries?.FirstOrDefault();
        if (artefact == null || artefact.Package == null)
        {
            return null;
        }

        var fileName = Path.GetFileName(artefact.Package.Link.LocalPath);

        return $"{TunaAdoptiumRoot}/"
            .AppendPathSegment(_asset.VersionData.Major)
            .AppendPathSegment(_series.Type)
            .AppendPathSegment(AdoptiumApiClient.GetTunaMirroredArchitecture(RuntimeInformation.OSArchitecture))
            .AppendPathSegment(AdoptiumApiClient.GetOs())
            .AppendPathSegment(fileName);
    }

    internal AdoptiumPackage(AdoptiumPackageSeries series, AdoptiumBinaryRelease asset, ILogger logger)
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

    public IEnumerable<ArtefactMirrorInfo>? GetMirrors()
    {
        var binary = (_asset.Binaries?.FirstOrDefault())
            ?? throw new NotSupportedException("Asset does not contain binary asset");

        if (binary.Package?.Checksum == null)
        {
            _logger.LogWarning("AdoptiumPackage: Release {ReleaseName} have no checksum", _asset.ReleaseName);
            return null;
        }

        var list = new List<ArtefactMirrorInfo>(2)
        {
            new()
            {
                Name = "github",
                IsOrigin = true,
                DownloadUri = binary.Package!.Link,
                Checksum = binary.Package!.Checksum!,
                ChecksumType = "sha256"
            }
        };

        var tunaUrl = GetTunaMirrorUrl();
        if (tunaUrl != null)
        {
            list.Add(new ArtefactMirrorInfo()
            {
                Name = "tuna",
                DownloadUri = new Uri(tunaUrl),
                Checksum = binary.Package!.Checksum!,
                ChecksumType = "sha256"
            });
        }

        return list.AsReadOnly();
    }
}

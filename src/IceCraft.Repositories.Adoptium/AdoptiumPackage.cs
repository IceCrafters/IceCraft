namespace IceCraft.Repositories.Adoptium;

using System.Collections.Generic;
using IceCraft.Core.Archive;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Repositories.Adoptium.Models;
using Microsoft.Extensions.Logging;
using Semver;

public class AdoptiumPackage : IPackage
{
    private readonly AdoptiumPackageSeries _series;
    private readonly AdoptiumBinaryRelease _asset;
    private readonly ILogger _logger;
    private readonly bool _addMirrors;

    internal AdoptiumPackage(AdoptiumPackageSeries series, AdoptiumBinaryRelease asset, ILogger logger, bool addMirrors)
    {
        _series = series;
        _asset = asset;
        _logger = logger;
        _addMirrors = addMirrors;
    }

    public IPackageSeries Series => _series;

    public RemoteArtefact GetArtefact()
    {
        var binary = (_asset.Binaries?.FirstOrDefault(x => x.Package is { Checksum: not null }))
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

        SemVersion version;
        if (asset.VersionData != null
            && SemVersion.TryParse(asset.VersionData.Semver, SemVersionStyles.Strict, out var semVer))
        {
            version = semVer;
        }
        else
        {
            version = new SemVersion(0, 0, 0, null, [asset.ReleaseName]);
        }

        return new PackageMeta(_series.Name,
            version: version,
            releaseDate: asset.Timestamp ?? asset.UpdatedAt ?? DateTime.MinValue,
            pluginInfo: new PackagePluginInfo(installerRef: "adoptium", configuratorRef: "adoptium"));
    }

    public IEnumerable<ArtefactMirrorInfo>? GetMirrors()
    {
        if (!_addMirrors)
        {
            return [AdoptiumMirrors.GetGitHubMirror(_asset)];
        }

        return AdoptiumMirrors.GetMirrors(_asset, _series.Type);
    }
}

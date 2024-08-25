namespace IceCraft.Extensions.DotNet.Archive;

using IceCraft.Core.Archive;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Dependency;
using IceCraft.Core.Archive.Packaging;
using Microsoft.Deployment.DotNet.Releases;
using Semver;

public class DotNetSdkPackage : IPackage
{
    private readonly SdkReleaseComponent _sdkRelease;
    private readonly DotNetSdkPackageSeries _series;
    private readonly ReleaseFile _releaseFile;

    public const string MetadataRuntimeVersion = "dotnet_runtime_ver";

    public DotNetSdkPackage(SdkReleaseComponent sdkRelease, ReleaseFile releaseFile, DotNetSdkPackageSeries series)
    {
        _sdkRelease = sdkRelease;
        _series = series;
        _releaseFile = releaseFile;
    }

    public IPackageSeries Series => _series;

    public RemoteArtefact GetArtefact()
    {
        return new RemoteArtefact
        {
            Checksum = _releaseFile.Hash,
            ChecksumType = "sha512"
        };
    }

    private static SemVersion GetSemanticVersion(ReleaseVersion releaseVersion)
    {
        string[] metaArray = !string.IsNullOrEmpty(releaseVersion.BuildMetadata)
            ? [releaseVersion.BuildMetadata]
            : [];
        
        string[] preReleaseArray = !string.IsNullOrEmpty(releaseVersion.Prerelease)
            ? [releaseVersion.Prerelease.Replace('.', '-')]
            : [];
        
        return new SemVersion(releaseVersion.Major,
            releaseVersion.Minor,
            releaseVersion.Patch,
            preReleaseArray,
            metaArray);
    }
    
    public PackageMeta GetMeta()
    {
        var version = _sdkRelease.Version;
        var rtVersion = _sdkRelease.RuntimeVersion;

        return new PackageMeta()
        {
            Id = _series.Name,
            Version = GetSemanticVersion(version),
            PluginInfo = new PackagePluginInfo("dotnet-sdk", "dotnet-sdk"),
            ReleaseDate = _sdkRelease.Release.ReleaseDate,
            AdditionalMetadata = new Dictionary<string, string>
            {
                { MetadataRuntimeVersion, new SemVersion(rtVersion.Major, rtVersion.Minor, rtVersion.Patch).ToString() }
            },
            ConflictsWith =
            [
                new DependencyReference($"dotnet-{_sdkRelease.Release.Product.ProductVersion}-runtime", SemVersionRange.All)
            ],
            Unitary = true
        };
    }

    public IEnumerable<ArtefactMirrorInfo>? GetMirrors()
    {
        yield return new ArtefactMirrorInfo()
        {
            Name = "origin",
            DownloadUri = _releaseFile.Address,
            IsOrigin = true
        };
    }
}

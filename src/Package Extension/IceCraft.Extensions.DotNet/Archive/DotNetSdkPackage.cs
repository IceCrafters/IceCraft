// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.DotNet.Archive;

using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;
using IceCraft.Api.Package.Data;
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

        var data = new PackageCustomDataDictionary();
        data.AddSerialize(MetadataRuntimeVersion, 
            new SemVersion(rtVersion.Major, rtVersion.Minor, rtVersion.Patch).ToString(),
            DotNetJsonContext.Default.String!);

        return new PackageMeta
        {
            Id = _series.Name,
            Version = GetSemanticVersion(version),
            PluginInfo = new PackagePluginInfo("dotnet-sdk", "dotnet-sdk"),
            ReleaseDate = _sdkRelease.Release.ReleaseDate,
            CustomData = data,
            ConflictsWith =
            [
                new DependencyReference($"dotnet-{_sdkRelease.Release.Product.ProductVersion}-runtime", SemVersionRange.All)
            ],
            Unitary = true,
            Transcript = new PackageTranscript
            {
                Authors = 
                [ 
                    new PackageAuthorInfo(".NET Foundation", "contact@dotnetfoundation.org"),
                    new PackageAuthorInfo("Microsoft Corporation"),
                    new PackageAuthorInfo(".NET contributors")
                ],
                Description = "An open-source framework for application and cloud services",
                Maintainer = new PackageAuthorInfo("Microsoft Corporation"),
                PluginMaintainer = new PackageAuthorInfo("IceCrafters"),
                License = "MIT"
            }
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

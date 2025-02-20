// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Repositories.Adoptium;
using System.Runtime.InteropServices;
using Flurl;
using IceCraft.Api.Archive.Artefacts;
using IceCraft.Repositories.Adoptium.Models;

public static class AdoptiumMirrors
{
    private const string TunaAdoptiumRoot = "https://mirrors.tuna.tsinghua.edu.cn/Adoptium";
    private const string CernNetAdoptiumRoot = "https://mirrors.cernet.edu.cn/Adoptium";

    internal static ArtefactMirrorInfo GetGitHubMirror(AdoptiumBinaryRelease release)
    {
        var artefact = (release.Binaries?.FirstOrDefault(x => x is { Package.Checksum: not null }))
            ?? throw new NotSupportedException("Asset does not contain binary asset");

        return new ArtefactMirrorInfo
        {
            Name = "github",
            IsOrigin = true,
            DownloadUri = artefact.Package!.Link
        };
    }

    internal static IEnumerable<ArtefactMirrorInfo>? GetMirrors(AdoptiumBinaryRelease release, string type)
    {
        var binary = (release.Binaries?.FirstOrDefault())
            ?? throw new NotSupportedException("Asset does not contain binary asset");

        if (binary.Package?.Checksum == null)
        {
            return null;
        }

        var checksum = binary.Package!.Checksum!;

        // Add original github mirror
        var list = new List<ArtefactMirrorInfo>(3)
        {
            GetGitHubMirror(release)
        };

        AddTunaMirror(list, release, type, checksum, "tuna", TunaAdoptiumRoot);
        AddTunaMirror(list, release, type, checksum, "mirrorz", CernNetAdoptiumRoot);

        return list.AsReadOnly();
    }

    public static string GetTunaMirroredArchitecture(Architecture architecture)
    {
        return architecture switch
        {
            Architecture.X86 => "x86",
            Architecture.X64 => "x64",
            Architecture.Arm or Architecture.Armv6 => "arm",
            Architecture.Arm64 => "aarch64",
            _ => throw new ArgumentException("Unsupported architecture.", nameof(architecture))
        };
    }

    private static void AddTunaMirror(List<ArtefactMirrorInfo> list, AdoptiumBinaryRelease release, string type, string checksum, string name, string urlRoot)
    {
        var tunaUrl = GetTunaMirrorUrl(release, type, urlRoot);
        if (tunaUrl != null)
        {
            list.Add(new ArtefactMirrorInfo
            {
                Name = name,
                DownloadUri = new Uri(tunaUrl)
            });
        }
    }

    private static string? GetTunaMirrorUrl(AdoptiumBinaryRelease release, string type, string urlRoot)
    {
        if (release.VersionData == null)
        {
            return null;
        }

        var artefact = release.Binaries?.FirstOrDefault();
        if (artefact?.Package == null)
        {
            return null;
        }

        var fileName = Path.GetFileName(artefact.Package.Link.LocalPath);

        return $"{urlRoot}/"
            .AppendPathSegment(release.VersionData.Major)
            .AppendPathSegment(type)
            .AppendPathSegment(GetTunaMirroredArchitecture(RuntimeInformation.OSArchitecture))
            .AppendPathSegment(AdoptiumApiClient.GetOs())
            .AppendPathSegment(fileName);
    }
}

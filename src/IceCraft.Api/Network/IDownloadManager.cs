// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Network;

using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Archive.Indexing;
using IceCraft.Api.Client;

public interface IDownloadManager
{
    Task<DownloadResult> DownloadAsync(Uri from, string toFile, IProgressedTask? downloadTask = null, string? fileName = null);
    Task<DownloadResult> DownloadAsync(Uri from, Stream toStream, IProgressedTask? downloadTask = null, string? fileName = null);

    [Obsolete("Use DownloadManagerExtensions.DownloadAtrefactAsync instead.")]
    Task<DownloadResult> DownloadAsync(ArtefactMirrorInfo fromMirror, Stream toStream, IProgressedTask? downloadTask = null, string? fileName = null);

    [Obsolete("Use DownloadManagerExtensions.DownloadAtrefactAsync instead.")]
    Task<DownloadResult> DownloadAsync(CachedPackageInfo packageInfo, Stream to, IProgressedTask? downloadTask = null,
        string? fileName = null);

    [Obsolete("Callers should manage temporary files themselves.")]
    Task<string> DownloadTemporaryArtefactAsync(CachedPackageInfo packageInfo, IProgressedTask? downloadTask = null, string? fileName = null);
    [Obsolete("Callers should manage temporary files themselves.")]
    Task<string> DownloadTemporaryArtefactAsync(ArtefactMirrorInfo mirror, IProgressedTask? downloadTask = null, string? fileName = null);

    /// <summary>
    /// Downloads an artefact to a temporary location, and validates its hash. Throws <see cref="IceCraft.Core.Util.KnownException"/>
    /// if hash mismatches.
    /// </summary>
    /// <param name="packageInfo">The package to download.</param>
    /// <param name="downloadTask">The progress reporting task to report to.</param>
    /// <returns>The path where the downloaded file is located.</returns>
    /// <exception cref="IceCraft.Core.Util.KnownException">Failed to validate artefact.</exception>
    [Obsolete("Callers should manage temporary files and compare checksums themselves.")]
    Task<string> DownloadTemporaryArtefactSecureAsync(CachedPackageInfo packageInfo, IProgressedTask? downloadTask = null, string? fileName = null);

    /// <summary>
    /// Downloads an artefact from the best mirror to a temporary location, and validates its hash. Throws <see cref="IceCraft.Core.Util.KnownException"/>
    /// if hash mismatches.
    /// </summary>
    /// <param name="artefact">The artefact information describing the package to be downloaded.</param>
    /// <param name="mirror">The package to download.</param>
    /// <param name="downloadTask">The progress reporting task to report to.</param>
    /// <returns>The path where the downloaded file is located.</returns>
    /// <exception cref="IceCraft.Core.Util.KnownException">Failed to validate artefact.</exception>
    [Obsolete("Callers should manage temporary files and compare checksums themselves.")]
    Task<string> DownloadTemporaryArtefactSecureAsync(IArtefactDefinition artefact,
        ArtefactMirrorInfo mirror,
        IProgressedTask? downloadTask = null, string? fileName = null);
}

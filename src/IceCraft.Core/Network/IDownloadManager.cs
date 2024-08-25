namespace IceCraft.Core.Network;
using System;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Platform;
using IceCraft.Core.Util;

public interface IDownloadManager
{
    Task<DownloadResult> DownloadAsync(Uri from, string toFile, IProgressedTask? downloadTask = null, string? fileName = null);
    Task<DownloadResult> DownloadAsync(Uri from, Stream toStream, IProgressedTask? downloadTask = null, string? fileName = null);
    Task<DownloadResult> DownloadAsync(ArtefactMirrorInfo fromMirror, Stream toStream, IProgressedTask? downloadTask = null, string? fileName = null);

    Task<DownloadResult> DownloadAsync(CachedPackageInfo packageInfo, Stream to, IProgressedTask? downloadTask = null,
        string? fileName = null);
    
    Task<string> DownloadTemporaryArtefactAsync(CachedPackageInfo packageInfo, IProgressedTask? downloadTask = null, string? fileName = null);
    Task<string> DownloadTemporaryArtefactAsync(ArtefactMirrorInfo mirror, IProgressedTask? downloadTask = null, string? fileName = null);

    /// <summary>
    /// Downloads an artefact to a temporary location, and validates its hash. Throws <see cref="KnownException"/>
    /// if hash mismatches.
    /// </summary>
    /// <param name="packageInfo">The package to download.</param>
    /// <param name="downloadTask">The progress reporting task to report to.</param>
    /// <returns>The path where the downloaded file is located.</returns>
    /// <exception cref="KnownException">Failed to validate artefact.</exception>
    Task<string> DownloadTemporaryArtefactSecureAsync(CachedPackageInfo packageInfo, IProgressedTask? downloadTask = null, string? fileName = null);
    
    /// <summary>
    /// Downloads an artefact from the best mirror to a temporary location, and validates its hash. Throws <see cref="KnownException"/>
    /// if hash mismatches.
    /// </summary>
    /// <param name="artefact">The artefact information describing the package to be downloaded.</param>
    /// <param name="mirror">The package to download.</param>
    /// <param name="downloadTask">The progress reporting task to report to.</param>
    /// <returns>The path where the downloaded file is located.</returns>
    /// <exception cref="KnownException">Failed to validate artefact.</exception>
    Task<string> DownloadTemporaryArtefactSecureAsync(RemoteArtefact artefact,
        ArtefactMirrorInfo mirror,
        IProgressedTask? downloadTask = null, string? fileName = null);
}

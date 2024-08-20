namespace IceCraft.Core.Network;
using System;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Util;

public interface IDownloadManager
{
    Task DownloadAsync(Uri from, string toFile, INetworkDownloadTask? downloadTask = null);
    Task DownloadAsync(Uri from, Stream toStream, INetworkDownloadTask? downloadTask = null);
    Task DownloadAsync(ArtefactMirrorInfo fromMirror, Stream toStream, INetworkDownloadTask? downloadTask = null);

    Task<string> DownloadTemporaryArtefactAsync(CachedPackageInfo packageInfo, INetworkDownloadTask? downloadTask = null);
    Task<string> DownloadTemporaryArtefactAsync(ArtefactMirrorInfo mirror, INetworkDownloadTask? downloadTask = null);

    /// <summary>
    /// Downloads an artefact to a temporary location, and validates its hash. Throws <see cref="KnownException"/>
    /// if hash mismatches.
    /// </summary>
    /// <param name="packageInfo">The package to download.</param>
    /// <param name="downloadTask">The progress reporting task to report to.</param>
    /// <returns>The path where the downloaded file is located.</returns>
    /// <exception cref="KnownException">Failed to validate artefact.</exception>
    Task<string> DownloadTemporaryArtefactSecureAsync(CachedPackageInfo packageInfo, INetworkDownloadTask? downloadTask = null);
    
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
        INetworkDownloadTask? downloadTask = null);
}

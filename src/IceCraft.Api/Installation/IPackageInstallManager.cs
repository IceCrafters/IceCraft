// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation;

using IceCraft.Api.Archive.Indexing;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;
using Semver;

public interface IPackageInstallManager
{
    Task InstallAsync(CachedPackageInfo packageInfo);
    Task InstallAsync(PackageMeta meta, string artefactPath);

    Task BulkInstallAsync(IAsyncEnumerable<KeyValuePair<PackageMeta, string>> packages, int expectedCount);

    Task<string> GetInstalledPackageDirectoryAsync(PackageMeta meta);

    /// <summary>
    /// Uninstalls the latest version of the specified package.
    /// </summary>
    /// <param name="meta">The package to uninstall.</param>
    Task UninstallAsync(PackageMeta meta);

    /// <summary>
    /// Determines whether at least one version of the specified package is installed.
    /// </summary>
    /// <param name="packageName">The name of the package.</param>
    /// <returns><see langword="true"/> if installed; otherwise, <see langword="false"/>.</returns>
    Task<bool> IsInstalledAsync(string packageName);
    
    /// <summary>
    /// Determines whether at least one version of the specified package is installed.
    /// </summary>
    /// <param name="packageName">The name of the package.</param>
    /// <param name="version">The version of the package.</param>
    /// <returns><see langword="true"/> if installed; otherwise, <see langword="false"/>.</returns>
    Task<bool> IsInstalledAsync(string packageName, string version);

    Task<bool> IsInstalledAsync(DependencyReference dependency);

    /// <summary>
    /// Queries the package database for any conflict between any installed package and the
    /// specified package.
    /// </summary>
    /// <param name="package">The package.</param>
    /// <returns><see langword="true"/> if there are no conflicts; otherwise, <see langword="false"/>.</returns>
    Task<bool> CheckForConflictAsync(PackageMeta package);

    /// <summary>
    /// Gets the <see cref="PackageMeta"/> instance that describes the metadata of the latest
    /// installed version of a given package.
    /// </summary>
    /// <param name="packageName">The name of the package to get metadata from.</param>
    /// <returns>The created metadata.</returns>
    Task<PackageMeta?> GetLatestMetaOrDefaultAsync(string packageName);

    /// <summary>
    /// Gets the <see cref="PackageMeta"/> instance that describes the metadata of the specified
    /// installed version of a given package.
    /// </summary>
    /// <param name="packageName">The name of the package to get metadata for.</param>
    /// <param name="version">The version of the package to get metadata for.</param>
    /// <returns>The created metadata.</returns>
    /// <exception cref="ArgumentException">No such package or version.</exception>
    Task<PackageMeta> GetMetaAsync(string packageName, SemVersion version);
    Task<PackageMeta?> TryGetMetaAsync(string packageName, SemVersion version);
    
    Task<PackageInstallationIndex?> GetIndexOrDefaultAsync(string metaId);

    Task RegisterVirtualPackageAsync(PackageMeta virtualMeta, PackageReference origin);
}

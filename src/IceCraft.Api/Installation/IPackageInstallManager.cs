// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation;

using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;
using Semver;

public interface IPackageInstallManager
{
    string GetUnsafePackageDirectory(PackageMeta meta);

    string GetInstalledPackageDirectory(PackageMeta meta);
    
    /// <summary>
    /// Determines whether the specified package is installed.
    /// </summary>
    /// <param name="meta">The metadata to query.</param>
    /// <returns><see langword="true"/> if metadata is not null and is present in database; otherwise, <see langword="false"/>.</returns>
    bool IsInstalled(PackageMeta? meta);
    
    /// <summary>
    /// Determines whether at least one version of the specified package is installed.
    /// </summary>
    /// <param name="packageName">The name of the package.</param>
    /// <returns><see langword="true"/> if installed; otherwise, <see langword="false"/>.</returns>
    bool IsInstalled(string packageName);
    
    /// <summary>
    /// Determines whether at least one version of the specified package is installed.
    /// </summary>
    /// <param name="packageName">The name of the package.</param>
    /// <param name="version">The version of the package.</param>
    /// <returns><see langword="true"/> if installed; otherwise, <see langword="false"/>.</returns>
    bool IsInstalled(string packageName, string version);

    bool IsInstalled(DependencyReference dependency);

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
    PackageMeta? GetLatestMetaOrDefault(string packageName);

    /// <summary>
    /// Gets the <see cref="PackageMeta"/> instance that describes the metadata of the latest
    /// installed version of a given package.
    /// </summary>
    /// <param name="packageName">The name of the package to get metadata from.</param>
    /// <param name="traceVirtualProvider">If <see langword="true"/>, the provider of a virtual package found under the specified ID will be returned.</param>
    /// <returns>The created metadata.</returns>
    PackageMeta? GetLatestMetaOrDefault(string packageName, bool traceVirtualProvider);
    
    /// <summary>
    /// Gets the <see cref="PackageMeta"/> instance that describes the metadata of the latest
    /// installed version of a given package.
    /// </summary>
    /// <param name="packageName">The name of the package to get metadata from.</param>
    /// <param name="cancellationToken">The token to cancel the operation.</param>
    /// <returns>The created metadata.</returns>
    Task<PackageMeta?> GetLatestMetaOrDefaultAsync(string packageName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the <see cref="PackageMeta"/> instance that describes the metadata of the specified
    /// installed version of a given package.
    /// </summary>
    /// <param name="packageName">The name of the package to get metadata for.</param>
    /// <param name="version">The version of the package to get metadata for.</param>
    /// <returns>The created metadata.</returns>
    /// <exception cref="ArgumentException">No such package or version.</exception>
    PackageMeta GetMeta(string packageName, SemVersion version);
    PackageMeta? GetMetaOrDefault(string packageName, SemVersion version);

    Task RegisterVirtualPackageAsync(PackageMeta virtualMeta, PackageReference origin);

    Task PutPackageAsync(InstalledPackageInfo info);
    Task UnregisterPackageAsync(PackageMeta meta);

    [Obsolete("Use ILocalPackageImporter.ImportEnvironment instead.")]
    void ImportEnvironment(PackageMeta meta);
}

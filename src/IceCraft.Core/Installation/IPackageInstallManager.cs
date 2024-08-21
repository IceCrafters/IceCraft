namespace IceCraft.Core.Installation;

using IceCraft.Core.Archive.Dependency;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Installation.Storage;
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
    /// <param name="version">The version of the package to get matadata for.</param>
    /// <returns>The created metadata.</returns>
    /// <exception cref="ArgumentException">No such package or version.</exception>
    Task<PackageMeta> GetMetaAsync(string packageName, SemVersion version);
    Task<PackageMeta?> TryGetMetaAsync(string packageName, SemVersion version);
    
    Task<PackageInstallationIndex?> GetIndexOrDefaultAsync(string metaId);
}

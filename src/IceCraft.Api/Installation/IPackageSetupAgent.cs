// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation;

using IceCraft.Api.Package;

/// <summary>
/// Defines a service that handles the installation, configuration and uninstallation process
/// of packages.
/// </summary>
public interface IPackageSetupAgent
{
    /// <summary>
    /// Uninstalls the latest version of the specified package.
    /// </summary>
    /// <param name="package">The package to uninstall.</param>
    Task UninstallAsync(PackageMeta package);

    /// <summary>
    /// Installs the specified package with the specified artefact.
    /// </summary>
    /// <param name="package">The package to install.</param>
    /// <param name="artefactPath">The artefact to install with.</param>
    Task InstallAsync(PackageMeta package, string artefactPath);

    /// <summary>
    /// Installs the specified packages.
    /// </summary>
    /// <param name="packages">The packages to install.</param>
    /// <param name="expectedCount">The expected count of packages that will be installed.</param>
    Task InstallManyAsync(IAsyncEnumerable<DueInstallTask> packages, int expectedCount);
}

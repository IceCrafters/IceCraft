// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation;

using IceCraft.Api.Exceptions;

/// <summary>
/// Defines a transient meta-service that queries for installation services and
/// manages their lifetime.
/// </summary>
public interface IPackageSetupLifetime
{
    /// <summary>
    /// Gets the <see cref="IPackageInstaller"/> implementation associated with the specified <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The identifier of the implementation to get.</param>
    /// <returns>The implementation found.</returns>
    /// <exception cref="PackageMetadataException">Unable to resolve the implementation.</exception>
    IPackageInstaller GetInstaller(string id);
    IPackageConfigurator GetConfigurator(string id);
    IArtefactPreprocessor GetPreprocessor(string id);
}

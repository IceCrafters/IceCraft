// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation;

using IceCraft.Api.Package;

public interface IPackageInstaller
{
    /// <summary>
    /// Expands the specified artefact file to the specified directory. The target directory
    /// does not have to be the final installation directory.
    /// </summary>
    /// <param name="artefactFile">The artefact file to expand.</param>
    /// <param name="targetDir">The directory to expand artefact to.</param>
    /// <param name="package">The package that is being installed.</param>
    Task ExpandPackageAsync(string artefactFile, string targetDir, PackageMeta package);

    /// <summary>
    /// Removes all files associated with the package.
    /// </summary>
    /// <param name="targetDir">The target directory.</param>
    /// <param name="package">The package that is being uninstalled.</param>
    /// <remarks>
    /// <para>
    /// Implementors <b>SHOULD</b> make sure that the target directory is either deleted or is ready
    /// to be deleted non-recursively. There is no point saving anything in it; if there are any left, IceCraft
    /// complains and recursively deletes everything.
    /// </para>
    /// </remarks>
    Task RemovePackageAsync(string targetDir, PackageMeta package);
}

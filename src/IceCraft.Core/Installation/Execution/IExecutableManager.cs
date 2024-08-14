namespace IceCraft.Core.Installation.Execution;

using IceCraft.Core.Archive.Packaging;

public interface IExecutableManager
{
    /// <summary>
    /// Creates a link in the executables directory for the current IceCraft instance, replacing
    /// an existing link if necessary, to a file whose is relative to the installation directory of the
    /// specified package.
    /// </summary>
    /// <param name="meta">The package metadata. The related package must be installed.</param>
    /// <param name="linkName">The name of the link to create.</param>
    /// <param name="from">The path, relative to the package installation directory, to the file to link to.</param>
    Task LinkExecutableAsync(PackageMeta meta, string linkName, string from);

    /// <summary>
    /// Deletes the link in the executables directory of the specified name.
    /// </summary>
    /// <param name="linkName"></param>
    /// <returns><see langword="true"/> if the link exists and have been removed; otherwise, <see langword="false"/>.</returns>
    Task<bool> UnlinkExecutableAsync(string linkName);
}

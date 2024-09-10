namespace IceCraft.Api.Installation;

public interface IPackageInstaller
{
    Task ExpandPackageAsync(string artefactFile, string targetDir);

    /// <summary>
    /// Removes all files associated with the package.
    /// </summary>
    /// <param name="targetDir">The target directory.</param>
    /// <remarks>
    /// <para>
    /// Implementors <b>SHOULD</b> make sure that the target directory is either deleted or is ready
    /// to be deleted non-recursively. There is no point saving anything in it; if there are any left, IceCraft
    /// complains and recursively deletes everything.
    /// </para>
    /// </remarks>
    Task RemovePackageAsync(string targetDir);
}

namespace IceCraft.Core.Installation.Execution;

using IceCraft.Core.Archive.Packaging;

public interface IExecutableManager
{
    Task RegisterAsync(PackageMeta meta, string linkName, string linkTo, EnvironmentVariableDictionary? variables = null);

    Task UnregisterAsync(PackageMeta meta, string linkName);

    Task SwitchAlternativeAsync(PackageMeta meta, string linkName);

    /// <summary>
    /// Deletes the link in the executables directory of the specified name.
    /// </summary>
    /// <param name="linkName"></param>
    /// <returns><see langword="true"/> if the link exists and have been removed; otherwise, <see langword="false"/>.</returns>
    Task<bool> UnlinkExecutableAsync(string linkName);
}

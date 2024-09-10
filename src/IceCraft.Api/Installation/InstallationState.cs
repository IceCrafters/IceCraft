namespace IceCraft.Api.Installation;

public enum InstallationState
{
    /// <summary>
    /// Package is not currently present on disk.
    /// </summary>
    None,
    /// <summary>
    /// Package is expanded to disk but is not configured yet.
    /// </summary>
    Expanded,
    /// <summary>
    /// Package is fully configured.
    /// </summary>
    Configured,
    /// <summary>
    /// Package is a virtual package. Implies that <see cref="InstalledPackageInfo.ProvidedBy"/> has a
    /// valid value.
    /// </summary>
    Virtual
}

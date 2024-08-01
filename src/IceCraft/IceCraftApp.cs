namespace IceCraft;

using System.Diagnostics;
using System.Reflection;
using IceCraft.Core;

internal static class IceCraftApp
{
    internal static readonly string UserDataDirecory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "IceCraft");
    internal static readonly string CachesDirectory = Path.Combine(UserDataDirecory, "caches");

    /// <summary>
    /// Gets the product version information of the IceCraft driver.
    /// </summary>
    /// <value>
    /// The <see cref="FileVersionInfo.ProductVersion"/> property acquired from the assembly file, 
    /// or <see langword="null"/> if assembly file is not found or does not contain version information.
    /// </value>
    internal static readonly string ProductVersion = GetProductVersion();

    private static string GetProductVersion()
    {
        var assemblyFile = Assembly.GetExecutingAssembly().Location;
        if (!File.Exists(assemblyFile))
        {
            return "<unknown>";
        }

        var versionInfo = FileVersionInfo.GetVersionInfo(assemblyFile);
        return versionInfo.ProductVersion ?? "<unknown>";
    }

    public static void Initialize()
    {
        Directory.CreateDirectory(UserDataDirecory);
    }
}

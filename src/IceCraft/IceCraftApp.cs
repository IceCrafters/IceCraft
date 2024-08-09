namespace IceCraft;

using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using IceCraft.Core.Platform;

internal class IceCraftApp : IFrontendApp
{
    private static readonly string UserDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "IceCraft");
    internal static readonly string CachesDirectory = Path.Combine(UserDataDirectory, "caches");

    private static readonly HttpClient HttpClient = new()
    {
        DefaultRequestHeaders =
        {
            UserAgent =
            {
                new ProductInfoHeaderValue("IceCraft", ProductVersion)
            }
        }
    };

    /// <summary>
    /// Gets the product version information of the IceCraft driver.
    /// </summary>
    /// <value>
    /// The <see cref="FileVersionInfo.ProductVersion"/> property acquired from the assembly file, 
    /// or <see langword="null"/> if assembly file is not found or does not contain version information.
    /// </value>
    internal static readonly string ProductVersion = GetProductVersion();

    public string ProductName => "IceCraft";

    string IFrontendApp.ProductVersion => ProductVersion;

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
        Directory.CreateDirectory(UserDataDirectory);
    }

    public HttpClient GetClient() => HttpClient;
}

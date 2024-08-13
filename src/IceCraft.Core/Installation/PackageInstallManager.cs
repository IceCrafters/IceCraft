namespace IceCraft.Core.Installation;

using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Network;
using IceCraft.Core.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class PackageInstallManager
{
    private readonly ILogger<PackageInstallManager> _logger;
    private readonly IDownloadManager _downloadManager;
    private readonly IFrontendApp _frontend;
    private readonly IServiceProvider _serviceProvider;

    private readonly string _packagesPath;

    public PackageInstallManager(ILogger<PackageInstallManager> logger, 
        IFrontendApp frontend,
        IDownloadManager downloadManager,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _frontend = frontend;
        _downloadManager = downloadManager;
        _serviceProvider = serviceProvider;

        _packagesPath = Path.Combine(frontend.DataBasePath, "packages");
    }

    public void CreateDirectories()
    {
        Directory.CreateDirectory(_packagesPath);
    }

    public async Task Install(CachedPackageInfo packageInfo)
    {
        var meta = packageInfo.Metadata;
        var installer = _serviceProvider.GetKeyedService<IPackageInstaller>(meta.PluginInfo.InstallerRef)
            ?? throw new ArgumentException($"Installer '{meta.PluginInfo.InstallerRef}' not found for package '{meta.Id}' '{meta.Version}'.", nameof(packageInfo));
            
        var tempFilePath = await _downloadManager.DownloadTemporaryArtefactAsync(packageInfo);
        await installer.InstallPackageAsync(tempFilePath, GetPackageDirectory(meta));
    }

    private string GetPackageDirectory(PackageMeta meta)
    {
        return Path.Combine(_packagesPath, CleanPath(meta.Id), CleanPath(meta.Version));
    }

    private static string CleanPath(string path)
    {
        return path.Replace('.', '_');
    }
}

namespace IceCraft.Core.Installation;

using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Installation.Storage;
using IceCraft.Core.Network;
using IceCraft.Core.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public partial class PackageInstallManager
{
    public const string PackagePath = "packages";

    private readonly ILogger<PackageInstallManager> _logger;
    private readonly IDownloadManager _downloadManager;
    private readonly IFrontendApp _frontend;
    private readonly IServiceProvider _serviceProvider;
    private readonly IPackageInstallDatabaseFactory _databaseFactory;

    private readonly string _packagesPath;

    public PackageInstallManager(ILogger<PackageInstallManager> logger, 
        IFrontendApp frontend,
        IDownloadManager downloadManager,
        IServiceProvider serviceProvider,
        IPackageInstallDatabaseFactory databaseFactory)
    {
        _logger = logger;
        _frontend = frontend;
        _downloadManager = downloadManager;
        _serviceProvider = serviceProvider;
        _databaseFactory = databaseFactory;

        _packagesPath = Path.Combine(frontend.DataBasePath, "packages");
    }

    public void CreateDirectories()
    {
        Directory.CreateDirectory(_packagesPath);
    }

    public async Task Install(CachedPackageInfo packageInfo)
    {
        var databaseTask = _databaseFactory.GetAsync();

        var meta = packageInfo.Metadata;
        var installer = _serviceProvider.GetKeyedService<IPackageInstaller>(meta.PluginInfo.InstallerRef)
            ?? throw new ArgumentException($"Installer '{meta.PluginInfo.InstallerRef}' not found for package '{meta.Id}' '{meta.Version}'.", nameof(packageInfo));
        var configurator = _serviceProvider.GetKeyedService<IPackageConfigurator>(meta.PluginInfo.ConfiguratorRef)
            ?? throw new ArgumentException($"Configurator '{meta.PluginInfo.ConfiguratorRef}' not found for package '{meta.Id}' '{meta.Version}'.", nameof(packageInfo));

        var tempFilePath = await _downloadManager.DownloadTemporaryArtefactAsync(packageInfo);
        var pkgDir = GetPackageDirectory(meta);
        await installer.InstallPackageAsync(tempFilePath, pkgDir);

        await configurator.ConfigurePackageAsync(pkgDir, meta);

        var database = await databaseTask;
        var entry = new InstalledPackageInfo()
        {
            Metadata = meta,
            State = InstallationState.Configured
        };
        database.Put(entry);
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

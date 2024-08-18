namespace IceCraft.Core.Installation;

using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Installation.Storage;
using IceCraft.Core.Network;
using IceCraft.Core.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public partial class PackageInstallManager : IPackageInstallManager
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

    public async Task InstallAsync(CachedPackageInfo packageInfo)
    {
        var meta = packageInfo.Metadata;
        var tempFilePath = await _downloadManager.DownloadTemporaryArtefactAsync(packageInfo);
        // TODO validate package.
        // TODO dependencies.
        await InstallAsync(meta, tempFilePath);
    }

    public async Task InstallAsync(PackageMeta meta, string artefactPath)
    {
        var pkgDir = GetPackageDirectory(meta);
        var installer = _serviceProvider.GetKeyedService<IPackageInstaller>(meta.PluginInfo.InstallerRef)
            ?? throw new ArgumentException($"Installer '{meta.PluginInfo.InstallerRef}' not found for package '{meta.Id}' '{meta.Version}'.", nameof(meta));
        var configurator = _serviceProvider.GetKeyedService<IPackageConfigurator>(meta.PluginInfo.ConfiguratorRef)
            ?? throw new ArgumentException($"Configurator '{meta.PluginInfo.ConfiguratorRef}' not found for package '{meta.Id}' '{meta.Version}'.", nameof(meta));

        _logger.LogInformation("Expanding package {Id}", meta.Id);
        await installer.ExpandPackageAsync(artefactPath, pkgDir);

        _logger.LogInformation("Configurating package {Id}", meta.Id);
        await configurator.ConfigurePackageAsync(pkgDir, meta);

        var database = await _databaseFactory.GetAsync();
        var entry = new InstalledPackageInfo()
        {
            Metadata = meta,
            State = InstallationState.Configured
        };
        database.Put(entry);
    }

    public async Task<string> GetInstalledPackageDirectoryAsync(PackageMeta meta)
    {
        var database = await _databaseFactory.GetAsync();
        if (!database.ContainsMeta(meta))
        {
            throw new ArgumentException("The specified package was not installed.", nameof(meta));
        }

        return GetPackageDirectory(meta);
    }

    private string GetPackageDirectory(PackageMeta meta)
    {
        return Path.Combine(_packagesPath, CleanPath(meta.Id), CleanPath(meta.Version));
    }

    private static string CleanPath(string path)
    {
        return path.Replace('.', '_');
    }

    public async Task UninstallAsync(PackageMeta meta)
    {
        var database = await _databaseFactory.GetAsync();
        if (!database.ContainsMeta(meta))
        {
            throw new ArgumentException("No such package meta installed.", nameof(meta));
        }

        var configurator = _serviceProvider.GetRequiredKeyedService<IPackageConfigurator>(meta.PluginInfo.ConfiguratorRef);
        var installer = _serviceProvider.GetRequiredKeyedService<IPackageInstaller>(meta.PluginInfo.InstallerRef);

        var directory = GetPackageDirectory(meta);

        _logger.LogInformation("Removing package");
        await configurator.UnconfigurePackageAsync(directory, meta);
        await installer.RemovePackageAsync(directory);

        database.Remove(meta.Id);
    }

    public async Task<bool> IsInstalledAsync(string packageName)
    {
        var database = await _databaseFactory.GetAsync();
        return database.ContainsKey(packageName);
    }

    public async Task<PackageMeta> GetMetaAsync(string packageName)
    {
        var database = await _databaseFactory.GetAsync();
        return database[packageName].Metadata;
    }
}

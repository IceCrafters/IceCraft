namespace IceCraft.Core.Installation;

using IceCraft.Core.Archive.Dependency;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Installation.Analysis;
using IceCraft.Core.Installation.Storage;
using IceCraft.Core.Network;
using IceCraft.Core.Platform;
using IceCraft.Core.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Semver;

public class PackageInstallManager : IPackageInstallManager
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
        CreateDirectories();
    }

    private void CreateDirectories()
    {
        Directory.CreateDirectory(_packagesPath);
    }

    public async Task BulkInstallAsync(IAsyncEnumerable<KeyValuePair<PackageMeta, string>> packages,
        int expectedCount)
    {
        // Key   : Meta 
        // Value : Expanded directory
        var dictionary = new Dictionary<PackageMeta, string>(expectedCount);

        var database = await _databaseFactory.GetAsync();

        // Configure and expand package.
        await foreach (var (meta, artefactPath) in packages)
        {
            var entry = new InstalledPackageInfo()
            {
                Metadata = meta,
                State = InstallationState.Expanded
            };

            var installer = _serviceProvider.GetKeyedService<IPackageInstaller>(meta.PluginInfo.InstallerRef)
                ?? throw new ArgumentException($"Installer '{meta.PluginInfo.InstallerRef}' not found for package '{meta.Id}' '{meta.Version}'.");
            var pkgDir = GetPackageDirectory(meta);

            // Expand package.
            Directory.CreateDirectory(pkgDir);
            _logger.LogInformation("Expanding package {Id}", meta.Id);
            try
            {
                await installer.ExpandPackageAsync(artefactPath, pkgDir);
            }
            catch (Exception ex)
            {
                throw new KnownException("Failed to expand package", ex);
            }

            database.Put(entry);
            dictionary.Add(meta, pkgDir);
        }

        await _databaseFactory.SaveAsync();

        // Configure package.
        // DependencyResolver resolves dependencies top-down, and thus the most safe way is to
        // do installations in reverse.
        foreach (var (meta, value) in dictionary.Reverse())
        {
            var configurator = _serviceProvider.GetKeyedService<IPackageConfigurator>(meta.PluginInfo.ConfiguratorRef)
                               ?? throw new ArgumentException($"Configurator '{meta.PluginInfo.ConfiguratorRef}' not found for package '{meta.Id}' '{meta.Version}'.");

            var entry = new InstalledPackageInfo()
            {
                Metadata = meta,
                State = InstallationState.Configured
            };

            _logger.LogInformation("Setting up package {Id}", meta.Id);
            try
            {
                await configurator.ConfigurePackageAsync(value, meta);
            }
            catch (Exception ex)
            {
                // Save the fact that the package is expanded but couldn't be set up.
                entry.State = InstallationState.Expanded;
                database.Put(entry);

                throw new KnownException("Failed to set up package", ex);
            }

            database.Put(entry);
        }

        await _databaseFactory.SaveAsync();
    }

    public async Task InstallAsync(CachedPackageInfo packageInfo)
    {
        var meta = packageInfo.Metadata;
        var tempFilePath = await _downloadManager.DownloadTemporaryArtefactAsync(packageInfo);
        await InstallAsync(meta, tempFilePath);
    }

    public async Task InstallAsync(PackageMeta meta, string artefactPath)
    {
        var database = await _databaseFactory.GetAsync();
        await InternalInstallAsync(database, meta, artefactPath);
        await _databaseFactory.SaveAsync();
    }

    private async Task InternalInstallAsync(IPackageInstallDatabase database, PackageMeta meta, string artefactPath)
    {
        var pkgDir = GetPackageDirectory(meta);
        var installer = _serviceProvider.GetKeyedService<IPackageInstaller>(meta.PluginInfo.InstallerRef)
            ?? throw new ArgumentException($"Installer '{meta.PluginInfo.InstallerRef}' not found for package '{meta.Id}' '{meta.Version}'.", nameof(meta));
        var configurator = _serviceProvider.GetKeyedService<IPackageConfigurator>(meta.PluginInfo.ConfiguratorRef)
            ?? throw new ArgumentException($"Configurator '{meta.PluginInfo.ConfiguratorRef}' not found for package '{meta.Id}' '{meta.Version}'.", nameof(meta));

        Directory.CreateDirectory(pkgDir);

        var entry = new InstalledPackageInfo()
        {
            Metadata = meta,
            State = InstallationState.Expanded
        };

        _logger.LogInformation("Expanding package {Id}", meta.Id);
        try
        {
            await installer.ExpandPackageAsync(artefactPath, pkgDir);
        }
        catch (Exception ex)
        {
            throw new KnownException("Failed to expand package", ex);
        }

        _logger.LogInformation("Setting up package {Id}", meta.Id);
        try
        {
            await configurator.ConfigurePackageAsync(pkgDir, meta);
        }
        catch (Exception ex)
        {
            // Save the fact that the package is expanded but couldn't be set up.
            entry.State = InstallationState.Expanded;
            database.Put(entry);

            throw new KnownException("Failed to set up package", ex);
        }

        entry.State = InstallationState.Configured;
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
        return Path.Combine(_packagesPath, CleanPath(meta.Id), CleanPath(meta.Version.ToString()));
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

        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory);
            }
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "IOException caught, retrying with recursive delete");
            _logger.LogWarning("TO AUTHOR OF {Name}: did you forget to clean the directory?", installer.GetType().FullName);
            Directory.Delete(directory, true);
        }

        database[meta.Id].Remove(meta.Version.ToString());
        await _databaseFactory.MaintainAndSaveAsync();
    }

    public async Task<bool> IsInstalledAsync(string packageName)
    {
        var database = await _databaseFactory.GetAsync();
        return database.ContainsKey(packageName);
    }

    public async Task<bool> IsInstalledAsync(string packageName, string version)
    {
        var database = await _databaseFactory.GetAsync();

        return database.TryGetValue(packageName, out var index)
               && index.ContainsKey(version);
    }

    public async Task<bool> IsInstalledAsync(DependencyReference dependency)
    {
        var database = await _databaseFactory.GetAsync();

        return database.TryGetValue(dependency.PackageId, out var index)
               && index.Values.Any(x => dependency.VersionRange.Contains(x.Metadata.Version));
    }

    public async Task<PackageMeta?> GetLatestMetaOrDefaultAsync(string packageName)
    {
        // TODO determine latest by semver
        var database = await _databaseFactory.GetAsync();
        return database[packageName].FirstOrDefault().Value?.Metadata;
    }

    public async Task<PackageMeta> GetMetaAsync(string packageName, SemVersion version)
    {
        var database = await _databaseFactory.GetAsync();
        return database[packageName][version.ToString()].Metadata;
    }

    public async Task<PackageMeta?> TryGetMetaAsync(string packageName, SemVersion version)
    {
        var database = await _databaseFactory.GetAsync();
        if (!database.TryGetValue(packageName, out var index))
        {
            return null;
        }

        if (!index!.TryGetValue(version.ToString(), out var result))
        {
            return null;
        }

        return result.Metadata;
    }

    public async Task<PackageInstallationIndex?> GetIndexOrDefaultAsync(string metaId)
    {
        var database = await _databaseFactory.GetAsync();

        return database.GetValueOrDefault(metaId);
    }

    public async Task RegisterVirtualPackageAsync(PackageMeta virtualMeta, PackageReference origin)
    {
        var database = await _databaseFactory.GetAsync();
        
        database.Put(new InstalledPackageInfo()
        {
           Metadata = virtualMeta,
           State = InstallationState.Virtual,
           ProvidedBy = origin
        });

        await _databaseFactory.MaintainAndSaveAsync();
    }
}

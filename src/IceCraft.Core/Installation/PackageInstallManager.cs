namespace IceCraft.Core.Installation;

using System.Diagnostics;
using System.IO.Abstractions;
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
    private readonly IFileSystem _fileSystem;

    private readonly string _packagesPath;

    public PackageInstallManager(ILogger<PackageInstallManager> logger,
        IFrontendApp frontend,
        IDownloadManager downloadManager,
        IServiceProvider serviceProvider,
        IPackageInstallDatabaseFactory databaseFactory,
        IFileSystem fileSystem)
    {
        _logger = logger;
        _frontend = frontend;
        _downloadManager = downloadManager;
        _serviceProvider = serviceProvider;
        _databaseFactory = databaseFactory;
        _fileSystem = fileSystem;

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
            var preprocessor = meta.PluginInfo.PreProcessorRef != null
            ? _serviceProvider.GetKeyedService<IArtefactPreprocessor>(meta.PluginInfo.PreProcessorRef)
                ?? throw new ArgumentException($"Preprocessor '{meta.PluginInfo.PreProcessorRef}' not found for package '{meta.Id}' '{meta.Version}'.", nameof(meta))
            : null;
            var pkgDir = GetPackageDirectory(meta);

            // If package is unitary, remove previous version.
            if (meta.Unitary
                && database.TryGetValue(meta.Id, out var index))
            {
                foreach (var (_, package) in index)
                {
                    await UninstallAsync(package.Metadata);
                }
            }

            // Expand package.
            await InternalExpandAsync(meta,
                artefactPath,
                pkgDir,
                _fileSystem,
                installer,
                preprocessor,
                _frontend.Output);

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

            _frontend.Output.Log("Setting up package {0} ({1})...", meta.Id, meta.Version);
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

    /// <summary>
    /// Expands the specified package and preprocesses it if available.
    /// </summary>
    /// <param name="meta">The package to install.</param>
    /// <param name="artefactPath">The path where the artefact is stored at.</param>
    /// <param name="expandTo">The path where the artefact will be expanded to.</param>
    /// <param name="fileSystem">The file system.</param>
    /// <param name="serviceProvider">The service provider where <see cref="IPackageInstaller"/> (and <see cref="IArtefactPreprocessor"/> if applicable) will be sourced from.</param>
    /// <param name="output"></param>
    /// <returns><see langword="true"/> if should continue; <see langword="false"/> if should store package as expanded not configured and abort.</returns>
    /// <exception cref="ArgumentException">The service provide failed to provide necessary services.</exception>
    /// <exception cref="KnownException">Expanding or reprocessing failed.</exception>
    internal static async Task InternalExpandAsync(PackageMeta meta, 
        string artefactPath, 
        string expandTo, 
        IFileSystem fileSystem,
        IPackageInstaller installer,
        IArtefactPreprocessor? preprocessor,
        IOutputAdapter? output = null)
    {
        string? tempExtraction = null;

        // If preprocessor is specified, create a temp directory that will await preprocessing
        if (preprocessor != null)
        {
            tempExtraction = fileSystem.Path.Combine(fileSystem.Path.GetTempPath(), 
                fileSystem.Path.GetRandomFileName());
                
            fileSystem.Directory.CreateDirectory(tempExtraction);
        }

        output?.Log("Expanding package {0} ({1})...", meta.Id, meta.Version);
        try
        {
            var objective = tempExtraction ?? expandTo;

            await installer.ExpandPackageAsync(artefactPath, objective);
        }
        catch (Exception ex)
        {
            throw new KnownException("Failed to expand package.", ex);
        }

        if (preprocessor != null)
        {
            // tempExtraction should absolutely NOT be null if preprocessor is found
            Debug.Assert(tempExtraction != null);

            output?.Log("Preprocessing package {0} ({1})...", meta.Id, meta.Version);

            try
            {
                await preprocessor.Preprocess(tempExtraction, expandTo, meta);
            }
            catch (Exception ex)
            {
                // Don't put anything in database because nothing is installed.
                
                // In case we left anything in the target get rid of it.
                if (fileSystem.Directory.Exists(expandTo))
                {
                    fileSystem.Directory.Delete(expandTo);
                }
                throw new KnownException("Failed to preprocess package", ex);
            }
        }
    }

    private async Task InternalInstallAsync(IPackageInstallDatabase database, PackageMeta meta, string artefactPath)
    {
        var pkgDir = GetPackageDirectory(meta);
        string? tempExtraction = null;

        var installer = _serviceProvider.GetKeyedService<IPackageInstaller>(meta.PluginInfo.InstallerRef)
            ?? throw new ArgumentException($"Installer '{meta.PluginInfo.InstallerRef}' not found for package '{meta.Id}' '{meta.Version}'.", nameof(meta));
        var configurator = _serviceProvider.GetKeyedService<IPackageConfigurator>(meta.PluginInfo.ConfiguratorRef)
            ?? throw new ArgumentException($"Configurator '{meta.PluginInfo.ConfiguratorRef}' not found for package '{meta.Id}' '{meta.Version}'.", nameof(meta));

        var preprocessor = meta.PluginInfo.PreProcessorRef != null
            ? _serviceProvider.GetKeyedService<IArtefactPreprocessor>(meta.PluginInfo.PreProcessorRef)
                ?? throw new ArgumentException($"Preprocessor '{meta.PluginInfo.PreProcessorRef}' not found for package '{meta.Id}' '{meta.Version}'.", nameof(meta))
            : null;

        Directory.CreateDirectory(pkgDir);

        // If preprocessor is specified, create a temp directory that will await preprocessing
        if (preprocessor != null)
        {
            tempExtraction = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            Directory.CreateDirectory(tempExtraction);
        }

        var entry = new InstalledPackageInfo()
        {
            Metadata = meta,
            State = InstallationState.Expanded
        };

        // If package is unitary, remove previous version.
        if (meta.Unitary
            && database.TryGetValue(meta.Id, out var index))
        {
            foreach (var (_, package) in index)
            {
                await UninstallAsync(package.Metadata);
            }
        }

        await InternalExpandAsync(meta,
            artefactPath,
            pkgDir,
            _fileSystem,
            installer,
            preprocessor,
            _frontend.Output);

        _frontend.Output.Log("Setting up package {0} ({1})...", meta.Id, meta.Version);
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
        var versionPart = meta.Unitary
            ? "unitary"
            : CleanPath(meta.Version.ToString());

        return Path.Combine(_packagesPath,
            CleanPath(meta.Id),
            versionPart);
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

        _frontend.Output.Log("Removing package {0} ({1})...", meta.Id, meta.Version);
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
            _frontend.Output.Warning(ex, "IOException caught, retrying with recursive delete");
            _frontend.Output.Warning("TO AUTHOR OF {0}: did you forget to clean the directory?", installer.GetType().FullName);
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

    public async Task<bool> CheckForConflictAsync(PackageMeta package)
    {
        if (package.ConflictsWith == null)
        {
            return true;
        }

        var database = await _databaseFactory.GetAsync();
        var isConflictFree = true;

        await Task.Run(() =>
        {
            foreach (var reference in package.ConflictsWith)
            {
                if (!database.TryGetValue(reference.PackageId, out var index))
                {
                    continue;
                }

                if (index.Any(x => reference.VersionRange.Contains(x.Value.Metadata.Version)
                    && !(x.Value.State == InstallationState.Virtual
                         && x.Value.ProvidedBy.HasValue
                         && x.Value.ProvidedBy.Value.DoesPointTo(package))))
                {
                    _frontend.Output.Warning("Package {0} ({1}) conflicts with {2} {3}",
                        package.Id,
                        package.Version,
                        reference.PackageId,
                        reference.VersionRange);

                    isConflictFree = false;
                    break;
                }
            }
        });

        return isConflictFree;
    }
}

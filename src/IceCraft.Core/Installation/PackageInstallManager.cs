// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Installation;

using System.Diagnostics;
using System.IO.Abstractions;
using IceCraft.Api.Client;
using IceCraft.Api.Exceptions;
using IceCraft.Api.Installation;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;
using Microsoft.Extensions.DependencyInjection;
using Semver;

public class PackageInstallManager : IPackageInstallManager
{
    public const string PackagePath = "packages";
    
    private readonly IFrontendApp _frontend;
    private readonly IServiceProvider _serviceProvider;
    private readonly IPackageInstallDatabaseFactory _databaseFactory;
    private readonly IFileSystem _fileSystem;

    private readonly string _packagesPath;

    public PackageInstallManager(IFrontendApp frontend,
        IServiceProvider serviceProvider,
        IPackageInstallDatabaseFactory databaseFactory,
        IFileSystem fileSystem)
    {
        _frontend = frontend;
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

    public async Task BulkInstallAsync(IAsyncEnumerable<DueInstallTask> packages,
        int expectedCount)
    {
        // Key   : Meta 
        // Value : Expanded directory
        var dictionary = new Dictionary<PackageMeta, string>(expectedCount);

        var database = _databaseFactory.Get();

        // Configure and expand package.
        await foreach (var installTask in packages)
        {
            var meta = installTask.Package;
            
            var entry = new InstalledPackageInfo
            {
                Metadata = installTask.Package,
                State = InstallationState.Expanded,
                IsExplicitlyInstalled = installTask.IsExplicit
            };

            var installer = _serviceProvider.GetKeyedService<IPackageInstaller>(meta.PluginInfo.InstallerRef)
                ?? throw new ArgumentException($"Installer '{meta.PluginInfo.InstallerRef}' not found for package '{meta.Id}' '{meta.Version}'.");
            var preprocessor = installTask.Package.PluginInfo.PreProcessorRef != null
            ? _serviceProvider.GetKeyedService<IArtefactPreprocessor>(meta.PluginInfo.PreProcessorRef)
                ?? throw new ArgumentException($"Preprocessor '{meta.PluginInfo.PreProcessorRef}' not found for package '{meta.Id}' '{meta.Version}'.", nameof(packages))
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
                installTask.ArtefactPath,
                pkgDir,
                _fileSystem,
                installer,
                preprocessor,
                _frontend.Output);

            await PutPackageAsync(entry);
            dictionary.Add(meta, pkgDir);
        }

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

            await PutPackageAsync(entry);
        }
    }
    
    public async Task InstallAsync(PackageMeta meta, string artefactPath)
    {
        var database = _databaseFactory.Get();
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
    /// <param name="preprocessor">The artefact preprocessor to preprocess the artefact with.</param>
    /// <param name="output">The output adapter to write user-friendly output to.</param>
    /// <param name="installer">The installer which is used to expand the artefact and remove the installed package files.</param>
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

        fileSystem.Directory.CreateDirectory(expandTo);

        output?.Log("Expanding package {0} ({1})...", meta.Id, meta.Version);
        try
        {
            var objective = tempExtraction ?? expandTo;

            fileSystem.Directory.CreateDirectory(objective);
            await installer.ExpandPackageAsync(artefactPath, objective, meta);
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
            var tempExtraction = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
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
            await PutPackageAsync(entry);

            throw new KnownException("Failed to set up package", ex);
        }

        entry.State = InstallationState.Configured;
        await PutPackageAsync(entry);
    }

    public string GetInstalledPackageDirectory(PackageMeta meta)
    {
        if (!IsInstalled(meta))
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
        if (!IsInstalled(meta))
        {
            throw new ArgumentException("No such package meta installed.", nameof(meta));
        }

        var configurator = _serviceProvider.GetRequiredKeyedService<IPackageConfigurator>(meta.PluginInfo.ConfiguratorRef);
        var installer = _serviceProvider.GetRequiredKeyedService<IPackageInstaller>(meta.PluginInfo.InstallerRef);

        var directory = GetPackageDirectory(meta);

        _frontend.Output.Log("Removing package {0} ({1})...", meta.Id, meta.Version);
        await configurator.UnconfigurePackageAsync(directory, meta);
        await installer.RemovePackageAsync(directory, meta);

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

        await UnregisterPackageAsync(meta);
    }

    public bool IsInstalled(PackageMeta meta)
    {
        var database = _databaseFactory.Get();

        return database.ContainsMeta(meta);
    }

    public bool IsInstalled(string packageName)
    {
        var database = _databaseFactory.Get();
        return database.ContainsKey(packageName);
    }

    public bool IsInstalled(string packageName, string version)
    {
        var database = _databaseFactory.Get();

        return database.TryGetValue(packageName, out var index)
               && index.ContainsKey(version);
    }

    public bool IsInstalled(DependencyReference dependency)
    {
        var database = _databaseFactory.Get();

        return database.TryGetValue(dependency.PackageId, out var index)
               && index.Values.Any(x => dependency.VersionRange.Contains(x.Metadata.Version));
    }

    public PackageMeta? GetLatestMetaOrDefault(string packageName)
    {
        // TODO determine latest by semver
        var database = _databaseFactory.Get();
        return database[packageName].FirstOrDefault().Value?.Metadata;
    }

    public PackageMeta GetMeta(string packageName, SemVersion version)
    {
        var database = _databaseFactory.Get();
        return database[packageName][version.ToString()].Metadata;
    }

    public PackageMeta? GetMetaOrDefault(string packageName, SemVersion version)
    {
        var database = _databaseFactory.Get();
        if (!database.TryGetValue(packageName, out var index))
        {
            return null;
        }

        return !index.TryGetValue(version.ToString(), out var result)
            ? null
            : result.Metadata;
    }

    public  PackageInstallationIndex? GetIndexOrDefault(string metaId)
    {
        var database = _databaseFactory.Get();

        return database.GetValueOrDefault(metaId);
    }

    public async Task RegisterVirtualPackageAsync(PackageMeta virtualMeta, PackageReference origin)
    {
        var database = _databaseFactory.Get();

        database.Put(new InstalledPackageInfo
        {
            Metadata = virtualMeta,
            State = InstallationState.Virtual,
            ProvidedBy = origin
        });

        await _databaseFactory.MaintainAndSaveAsync();
    }

    public async Task PutPackageAsync(InstalledPackageInfo info)
    {
        var database = _databaseFactory.Get();
        database.Put(info);
        
        await _databaseFactory.SaveAsync();
    }

    public async Task UnregisterPackageAsync(PackageMeta meta)
    {
        var database = _databaseFactory.Get();
        
        database[meta.Id].Remove(meta.Version.ToString());
        await _databaseFactory.MaintainAndSaveAsync();
    }

    public async Task<bool> CheckForConflictAsync(PackageMeta package)
    {
        if (package.ConflictsWith == null)
        {
            return true;
        }

        var database = _databaseFactory.Get();
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
                    && !(x.Value is { State: InstallationState.Virtual, ProvidedBy: not null }
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

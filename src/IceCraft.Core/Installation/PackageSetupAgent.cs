// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Installation;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Threading.Tasks;
using IceCraft.Api.Client;
using IceCraft.Api.Exceptions;
using IceCraft.Api.Installation;
using IceCraft.Api.Installation.Database;
using IceCraft.Api.Package;

public class PackageSetupAgent : IPackageSetupAgent
{
    private readonly IPackageInstallManager _installManager;
    private readonly IFileSystem _fileSystem;
    private readonly IFrontendApp _frontend;
    private readonly ILocalDatabaseMutator _mutator;
    private readonly IPackageSetupLifetime _lifetime;

    public PackageSetupAgent(IPackageInstallManager installManager,
        IFileSystem fileSystem,
        IFrontendApp frontend,
        ILocalDatabaseMutator mutator,
        IPackageSetupLifetime lifetime)
    {
        _installManager = installManager;
        _fileSystem = fileSystem;
        _frontend = frontend;
        _mutator = mutator;
        _lifetime = lifetime;
    }

    public async Task InstallManyAsync(IAsyncEnumerable<DueInstallTask> packages, int expectedCount)
    {
        // Configure and expand package.
        await foreach (var installTask in packages)
        {
            var meta = installTask.Package;

            await InternalInstallAsync(_mutator, meta, installTask.ArtefactPath);
        }
    }

    public async Task InstallAsync(PackageMeta meta, string artefactPath)
    {
        await InternalInstallAsync(_mutator, meta, artefactPath);
        await _mutator.StoreAsync();
    }

    public async Task UninstallAsync(PackageMeta meta)
    {
        if (!_installManager.IsInstalled(meta))
        {
            throw new ArgumentException("No such package meta installed.", nameof(meta));
        }

        var configurator = _lifetime.GetConfigurator(meta.PluginInfo.ConfiguratorRef);
        var installer = _lifetime.GetInstaller(meta.PluginInfo.InstallerRef);

        var directory = _installManager.GetUnsafePackageDirectory(meta);

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

        await _installManager.UnregisterPackageAsync(meta);
    }

    public async Task ReconfigureAsync(PackageMeta package)
    {
        var configurator = _lifetime.GetConfigurator(package.PluginInfo.ConfiguratorRef);
        var installDir = _installManager.GetInstalledPackageDirectory(package);
        
        await configurator.UnconfigurePackageAsync(installDir, package);
        await configurator.ConfigurePackageAsync(installDir, package);
    }

    #region Installation implementation

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

    private async Task InternalInstallAsync(ILocalDatabaseMutator mutator, PackageMeta meta, string artefactPath)
    {
        // Create a scope for each package installation task.

        var pkgDir = _installManager.GetUnsafePackageDirectory(meta);

        var installer = _lifetime.GetInstaller(meta.PluginInfo.InstallerRef);
        var configurator = _lifetime.GetConfigurator(meta.PluginInfo.ConfiguratorRef);
        var preprocessor = meta.PluginInfo.PreProcessorRef != null
            ? _lifetime.GetPreprocessor(meta.PluginInfo.PreProcessorRef)
            : null;

        Directory.CreateDirectory(pkgDir);

        // If preprocessor is specified, create a temp directory that will await preprocessing
        if (preprocessor != null)
        {
            var tempExtraction = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempExtraction);
        }

        var entry = new InstalledPackageInfo()
        {
            Metadata = meta,
            State = InstallationState.Expanded
        };

        // If package is unitary, remove previous version.
        if (meta.Unitary
            && mutator.ContainsPackage(meta.Id))
        {
            foreach (var package in mutator.EnumeratePackages(meta.Id))
            {
                await UninstallAsync(package);
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
            await _installManager.PutPackageAsync(entry);

            throw new KnownException("Failed to set up package", ex);
        }

        entry.State = InstallationState.Configured;
        await _installManager.PutPackageAsync(entry);
    }

    #endregion
}

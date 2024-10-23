// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Client;

using System;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Threading.Tasks;
using IceCraft.Api.Client;
using IceCraft.Api.Exceptions;
using IceCraft.Api.Installation;
using IceCraft.Api.Network;
using IceCraft.Api.Package;
using IceCraft.Api.Plugin.Commands;
using IceCraft.Extensions.CentralRepo.Impl;
using IceCraft.Extensions.CentralRepo.Runtime;

internal class CsrBuildCommand
{
    private readonly MashiroRuntime _runtime;
    private readonly MashiroStatePool _statePool;
    private readonly IDownloadManager _downloadManager;
    private readonly IMirrorSearcher _mirrorSearcher;
    private readonly IFrontendApp _frontendApp;
    private IPluginArgument<FileInfo>? _argFile;

    public CsrBuildCommand(MashiroRuntime runtime, MashiroStatePool statePool, IDownloadManager downloadManager, IMirrorSearcher mirrorSearcher, IFrontendApp frontendApp)
    {
        _runtime = runtime;
        _statePool = statePool;
        _downloadManager = downloadManager;
        _mirrorSearcher = mirrorSearcher;
        _frontendApp = frontendApp;
    }

    public void Configure(IPluginCommand command)
    {
        _argFile = command.AddArgument<FileInfo>("file", "The script file to build.");

        command.SetHandler(Run);
    }

    private async Task<int> Run(IPluginCommandContext context)
    {
        if (_argFile == null)
        {
            throw new InvalidOperationException("File argument must be initialised");
        }
        var file = context.GetArgument(_argFile);
        if (file == null || !file.Exists)
        {
            throw new KnownException("Invalid file!");
        }

        // Setup mashiro engine
        using var lifetime = await _runtime.CreateStateLifetimeAsync(file.FullName);

        lifetime.State.EnsureMetadata();
        var meta = lifetime.State.GetPackageMeta();
        var mirrors = lifetime.State.GetMirrors();
        var best = await _mirrorSearcher.GetBestMirrorAsync(mirrors);

        // Create temporary file
        var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var tempExtPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempExtPath);

        _statePool.Inject(meta!, lifetime);

        using (var stream = File.Create(tempFilePath))
        {
            await _downloadManager.DownloadArtefactAsync(best!, stream);
        }

        // Setup mashiro services
        var installer = new MashiroInstaller(_statePool);
        var configurator = new MashiroConfigurator(_statePool);
        MashiroPreprocessor? preprocessor = null;

        if (meta!.PluginInfo.PreProcessorRef != null)
        {
            preprocessor = new MashiroPreprocessor(_statePool);
        }

        // Expand and configure
        await InternalExpandAsync(meta!, tempFilePath, tempExtPath, new FileSystem(), installer, preprocessor, _frontendApp.Output);
        await configurator.ConfigurePackageAsync(tempFilePath, meta);

        return 0;
    }

    // TODO get rid of this
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
}

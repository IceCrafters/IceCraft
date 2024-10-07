// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Runtime;

using Acornima.Ast;
using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Exceptions;
using IceCraft.Api.Package;
using IceCraft.Api.Platform;
using IceCraft.Extensions.CentralRepo.Api;
using IceCraft.Extensions.CentralRepo.Network;
using IceCraft.Extensions.CentralRepo.Runtime.Security;
using Jint;
using Microsoft.Extensions.DependencyInjection;

public class MashiroState : IDisposable
{
    private readonly Engine _engine;
    private readonly Prepared<Script> _preparedScript;
    private readonly IServiceProvider _serviceProvider;
    
    private readonly List<ArtefactMirrorInfo> _mirrors = [];
    private readonly ContextApiRoot _apiRoot = new();

    private bool _metadataRan;
    
    internal MashiroRuntime.ExpandPackageDelegate? ExpandPackageDelegate { get; private set; }
    internal MashiroRuntime.RemovePackageDelegate? RemovePackageDelegate { get; private set; }
    internal MashiroRuntime.ConfigureDelegate? ConfigurePackageDelegate { get; private set; }
    internal MashiroRuntime.ConfigureDelegate? ExportEnvironmentDelegate { get; private set; }
    internal MashiroRuntime.UnConfigureDelegate? UnConfigurePackageDelegate { get; private set; }
    internal MashiroRuntime.OnPreprocessDelegate? PreprocessPackageDelegate { get; private set; }

    public MashiroState(IServiceProvider serviceProvider, Engine engine, Prepared<Script> preparedScript,
        string? fileName = null)
    {
        _engine = engine;
        _preparedScript = preparedScript;
        _serviceProvider = serviceProvider;
        FileName = fileName;
    }

    private PackageMeta? PackageMeta { get; set; }

    public IArtefactDefinition? ArtefactDefinition { get; private set; }

    public ArtefactMirrorInfo? Origin { get; private set; }
    
    public string? FileName { get; }

    #region Mashiro functions

    private void MashiroSetMeta(PackageMeta packageMeta)
    {
        PackageMeta = packageMeta;
    }

    private void MashiroSha512Sum(string sha)
    {
        ArtefactDefinition = new HashedArtefact("sha512", sha);
    }

    private void MashiroVoidSum()
    {
        ArtefactDefinition = new VolatileArtefact();
    }

    private void MashiroSetOrigin(string origin)
    {
        if (Origin != null)
        {
            throw new InvalidOperationException("Origin is already set");
        }
        
        Origin = new ArtefactMirrorInfo
        {
            Name = origin,
            IsOrigin = true,
            DownloadUri = new Uri(origin)
        };
    }
    
    private void MashiroAddMirror(string mirrorName, string url)
    {
        _mirrors.Add(new ArtefactMirrorInfo
        {
            Name = mirrorName,
            IsOrigin = false,
            DownloadUri = new Uri(url)
        });
    }

    private void MashiroOnExpand(MashiroRuntime.ExpandPackageDelegate action)
    {
        ExpandPackageDelegate = action;
    }

    private void MashiroOnRemove(MashiroRuntime.RemovePackageDelegate action)
    {
        RemovePackageDelegate = action;
    }
    
    private void MashiroOnPreprocess(MashiroRuntime.OnPreprocessDelegate action)
    {
        PreprocessPackageDelegate = action;
    }


    private void MashiroOnConfigure(MashiroRuntime.ConfigureDelegate action)
    {
        ConfigurePackageDelegate = action;
    }
    
    private void MashiroOnUnConfigure(MashiroRuntime.UnConfigureDelegate action)
    {
        UnConfigurePackageDelegate = action;
    }
    
    private void MashiroOnExportEnvironment(MashiroRuntime.ConfigureDelegate action)
    {
        ExportEnvironmentDelegate = action;
    }

    #endregion

    public void EnsureMetadata()
    {
        if (_metadataRan)
        {
            return;
        }
        
        _apiRoot.DoContext(ExecutionContextType.Metadata,
            () => _engine.Execute(_preparedScript));
        _metadataRan = true;
    }

    public IList<ArtefactMirrorInfo> GetMirrors()
    {
        if (Origin == null)
        {
            throw new KnownInvalidOperationException("Origin is not set");
        }
        
        var list = new List<ArtefactMirrorInfo>(_mirrors.Count + 1);
        list.AddRange(_mirrors);
        list.Add(Origin);
        return list;
    }

    public PackageMeta? GetPackageMeta()
    {
        if (PackageMeta == null)
        {
            return null;
        }

        var pluginInfo = new PackagePluginInfo("mashiro",
            "mashiro",
            PreprocessPackageDelegate != null ? "mashiro" : null);
        return PackageMeta with
        {
            PluginInfo = pluginInfo
        };
    }

    internal void AddFunctions()
    {
        _engine.SetValue("setMeta", MashiroSetMeta);
        _engine.SetValue("sha512sum", MashiroSha512Sum);
        _engine.SetValue("voidsum", MashiroVoidSum);
        _engine.SetValue("setOrigin", MashiroSetOrigin);
        _engine.SetValue("addMirror", MashiroAddMirror);
        
        _engine.SetValue("onExpand", MashiroOnExpand);
        _engine.SetValue("onRemove", MashiroOnRemove);
        _engine.SetValue("onConfigure", MashiroOnConfigure);
        _engine.SetValue("onUnConfigure", MashiroOnUnConfigure);
        _engine.SetValue("onExportEnv", MashiroOnExportEnvironment);
        _engine.SetValue("onPreprocess", MashiroOnPreprocess);

        _engine.SetValue("Fs", new MashiroFs(_apiRoot));
        _engine.SetValue("CompressedArchive", new MashiroCompressedArchive(_apiRoot));
        _engine.SetValue("Os", new MashiroOs(_apiRoot));
        _engine.SetValue("Binary", new MashiroBinary(_apiRoot,
            _serviceProvider.GetRequiredService<IExecutableManager>(),
            this));
        _engine.SetValue("Packages", new MashiroPackages(_apiRoot,
            _serviceProvider, this));
        _engine.SetValue("Assets", new MashiroAssets(_apiRoot,
            _serviceProvider.GetRequiredService<IRemoteRepositoryManager>()));
    }

    public void Dispose()
    {
        _engine.Dispose();
        GC.SuppressFinalize(this);
    }

    private static void VerifyDelegate<T>(T? dlg, string name)
        where T: Delegate
    {
        if (dlg == null)
        {
            throw new InvalidOperationException($"{name} must be called and set with a required value.");
        }
    }
    
    public void VerifyRequiredDelegates()
    {
        VerifyDelegate(ExpandPackageDelegate, "onExpand");
        VerifyDelegate(RemovePackageDelegate, "onRemove");
        VerifyDelegate(ConfigurePackageDelegate, "onConfigure");
        VerifyDelegate(UnConfigurePackageDelegate, "onUnConfigure");
        VerifyDelegate(ExportEnvironmentDelegate, "onExportEnv");
    }

    public void DoContext(ExecutionContextType contextType, Action action)
    {
        _apiRoot.DoContext(contextType, action);
    }
    
    public async Task DoContextAsync(ExecutionContextType contextType, Func<Task> action)
    {
        await _apiRoot.DoContextAsync(contextType, action);
    }
}
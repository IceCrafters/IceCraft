// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Runtime;

using Acornima.Ast;
using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Client;
using IceCraft.Api.Exceptions;
using IceCraft.Api.Package;
using IceCraft.Extensions.CentralRepo.Runtime.Security;
using Jint;

public class MashiroState : IDisposable
{
    private readonly Engine _engine;
    
    private readonly List<ArtefactMirrorInfo> _mirrors = [];
    private readonly IMashiroApiProvider _apiProvider;
    private readonly IFrontendApp _frontendApp;
    private readonly ContextApiRoot _context;
    private Prepared<Script>? _script;

    private bool _metadataRan;
    
    internal MashiroRuntime.ExpandPackageDelegate? ExpandPackageDelegate { get; private set; }
    internal MashiroRuntime.RemovePackageDelegate? RemovePackageDelegate { get; private set; }
    internal MashiroRuntime.ConfigureDelegate? ConfigurePackageDelegate { get; private set; }
    internal MashiroRuntime.ConfigureDelegate? ExportEnvironmentDelegate { get; private set; }
    internal MashiroRuntime.UnConfigureDelegate? UnConfigurePackageDelegate { get; private set; }
    internal MashiroRuntime.OnPreprocessDelegate? PreprocessPackageDelegate { get; private set; }

    public MashiroState(IMashiroApiProvider apiProvider, 
        Engine engine,
        IFrontendApp frontendApp,
        ContextApiRoot context)
    {
        _engine = engine;
        _apiProvider = apiProvider;
        _frontendApp = frontendApp;
        _context = context;
    }

    private PackageMeta? PackageMeta { get; set; }

    public IArtefactDefinition? ArtefactDefinition { get; private set; }

    public ArtefactMirrorInfo? Origin { get; private set; }

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

    public void SetScript(string code, string name)
    {
        SetScript(Engine.PrepareScript(code, name));
    }
    
    public void SetScript(Prepared<Script> script)
    {
        _script = script;
    }

    public void EnsureMetadata()
    {
        if (_metadataRan)
        {
            return;
        }

        if (!_script.HasValue)
        {
            throw new InvalidOperationException("Script not set. Call SetScript() first.");
        }
        
        _context.DoContext(ExecutionContextType.Metadata,
            () => _engine.Execute(_script.Value));
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

    internal void AddApis()
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

        _engine.SetValue("Fs", _apiProvider.Fs);
        _engine.SetValue("CompressedArchive", _apiProvider.CompressedArchive);
        _engine.SetValue("Os", _apiProvider.Os);
        _engine.SetValue("Binary", _apiProvider.Binary);
        _engine.SetValue("Packages", _apiProvider.Packages);
        _engine.SetValue("Assets", _apiProvider.Assets);
        _engine.SetValue("mconsole", _apiProvider.MConsole);

        _engine.SetValue("AppBasePath", 
            _frontendApp.DataBasePath);
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
        _context.DoContext(contextType, action);
    }
    
    public async Task DoContextAsync(ExecutionContextType contextType, Func<Task> action)
    {
        await _context.DoContextAsync(contextType, action);
    }
}
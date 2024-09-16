namespace IceCraft.Extensions.CentralRepo.Runtime;

using Acornima.Ast;
using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Exceptions;
using IceCraft.Api.Package;
using Jint;
using Jint.Runtime;

public class MashiroState : IDisposable
{
    private readonly Engine _engine;
    private readonly Prepared<Script> _preparedScript;
    
    private readonly List<ArtefactMirrorInfo> _mirrors = [];
    
    internal MashiroRuntime.ExpandPackageAsync? ExpandPackageDelegate { get; private set; }
    internal MashiroRuntime.RemovePackageAsync? RemovePackageDelegate { get; private set; }
    internal MashiroRuntime.OnPreprocessAsync? PreprocessPackageDelegate { get; private set; }

    public MashiroState(Engine engine, Prepared<Script> preparedScript)
    {
        _engine = engine;
        _preparedScript = preparedScript;
    }

    private PackageMeta? PackageMeta { get; set; }

    public RemoteArtefact? RemoteArtefact { get; private set; }

    public ArtefactMirrorInfo? Origin { get; private set; }

    #region Mashiro functions

    private void MashiroSetMeta(PackageMeta packageMeta)
    {
        PackageMeta = packageMeta;
    }

    private void MashiroSha512Sum(string sha)
    {
        RemoteArtefact = new RemoteArtefact("sha512", sha);
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

    private void MashiroOnExpand(MashiroRuntime.ExpandPackageAsync action)
    {
        ExpandPackageDelegate = action;
    }

    private void MashiroOnRemove(MashiroRuntime.RemovePackageAsync action)
    {
        RemovePackageDelegate = action;
    }

    private void MashiroOnConfigure(MashiroRuntime.OnPreprocessAsync action)
    {
        PreprocessPackageDelegate = action;
    }

    #endregion

    public void RunMetadata()
    {
        _engine.Execute(_preparedScript);
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
        _engine.SetValue("setOrigin", MashiroSetOrigin);
        _engine.SetValue("addMirror", MashiroAddMirror);
        
        _engine.SetValue("onExpand", MashiroOnExpand);
        _engine.SetValue("onRemove", MashiroOnRemove);
        _engine.SetValue("onConfigure", MashiroOnConfigure);
    }

    public void Dispose()
    {
        _engine.Dispose();
        GC.SuppressFinalize(this);
    }
}
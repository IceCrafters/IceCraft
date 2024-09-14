namespace IceCraft.Extensions.CentralRepo.Runtime;

using Acornima.Ast;
using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Package;
using Jint;

public class MashiroState : IDisposable
{
    private readonly Engine _engine;
    private readonly Prepared<Script> _preparedScript;
    
    private readonly List<ArtefactMirrorInfo> _mirrors = [];
    
    internal MashiroRuntime.ExpandPackageAsync? ExpandPackageDelegate { get; private set; }
    internal MashiroRuntime.RemovePackageAsync? RemovePackageDelegate { get; private set; }

    public MashiroState(Engine engine, Prepared<Script> preparedScript)
    {
        _engine = engine;
        _preparedScript = preparedScript;
    }

    public PackageMeta? PackageMeta { get; private set; }

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
            IsOrigin = true,
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

    #endregion

    internal void AddFunctions()
    {
        _engine.SetValue("setMeta", MashiroSetMeta);
        _engine.SetValue("sha512sum", MashiroSha512Sum);
        _engine.SetValue("setOrigin", MashiroSetOrigin);
        _engine.SetValue("addMirror", MashiroAddMirror);
        
        _engine.SetValue("onExpand", MashiroOnExpand);
        _engine.SetValue("onRemove", MashiroOnRemove);
    }

    public void Dispose()
    {
        _engine.Dispose();
        GC.SuppressFinalize(this);
    }
}
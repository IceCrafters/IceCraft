namespace IceCraft.Extensions.CentralRepo.Runtime;

using System.Management.Automation;
using System.Management.Automation.Runspaces;
using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Exceptions;
using IceCraft.Api.Package;
using Jint;
using Semver;

public class MashiroState : IDisposable
{
    private readonly Engine _engine;

    public MashiroState(Engine engine)
    {
        _engine = engine;
    }
    
    public PackageMeta? PackageMeta { get; private set; }

    public void SetMeta(PackageMeta packageMeta)
    {
        PackageMeta = packageMeta;
    }

    public void Dispose()
    {
        _engine.Dispose();
        GC.SuppressFinalize(this);
    }
}
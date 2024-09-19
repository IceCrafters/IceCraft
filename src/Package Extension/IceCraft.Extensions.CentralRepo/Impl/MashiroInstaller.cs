namespace IceCraft.Extensions.CentralRepo.Impl;

using IceCraft.Api.Installation;
using IceCraft.Api.Package;
using IceCraft.Extensions.CentralRepo.Runtime;

public class MashiroInstaller : IPackageInstaller
{
    private readonly MashiroStatePool _statePool;

    public MashiroInstaller(MashiroStatePool statePool)
    {
        _statePool = statePool;
    }
    
    public async Task ExpandPackageAsync(string artefactFile, string targetDir, PackageMeta package)
    {
        var state = await _statePool.GetAsync(package);
        state.RunMetadata();
        if (state.ExpandPackageDelegate == null)
        {
            throw new InvalidOperationException($"No preprocessor registered for package {package.Id} ({package.Version})");
        }
        
        await state.ExpandPackageDelegate(artefactFile, targetDir);
    }

    public async Task RemovePackageAsync(string targetDir, PackageMeta package)
    {
        var state = await _statePool.GetAsync(package);
        state.RunMetadata();
        if (state.RemovePackageDelegate == null)
        {
            throw new InvalidOperationException($"No preprocessor registered for package {package.Id} ({package.Version})");
        }
        
        await state.RemovePackageDelegate(targetDir);
    }
}
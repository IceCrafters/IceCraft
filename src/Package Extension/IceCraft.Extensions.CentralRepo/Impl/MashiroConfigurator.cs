namespace IceCraft.Extensions.CentralRepo.Impl;

using IceCraft.Api.Installation;
using IceCraft.Api.Package;
using IceCraft.Extensions.CentralRepo.Runtime;

public class MashiroConfigurator : IPackageConfigurator
{
    private readonly MashiroStatePool _statePool;

    public MashiroConfigurator(MashiroStatePool statePool)
    {
        _statePool = statePool;
    }
    
    public async Task ConfigurePackageAsync(string installDir, PackageMeta meta)
    {
        var state = await _statePool.GetAsync(meta);
        state.RunMetadata();
        if (state.ConfigurePackageDelegate == null)
        {
            throw new InvalidOperationException($"No preprocessor registered for package {meta.Id} ({meta.Version})");
        }
        
        await state.ConfigurePackageDelegate(installDir);
    }

    public async Task UnconfigurePackageAsync(string installDir, PackageMeta meta)
    {
        var state = await _statePool.GetAsync(meta);
        state.RunMetadata();
        if (state.UnConfigurePackageDelegate == null)
        {
            throw new InvalidOperationException($"No preprocessor registered for package {meta.Id} ({meta.Version})");
        }
        
        await state.UnConfigurePackageDelegate(installDir);
    }
}
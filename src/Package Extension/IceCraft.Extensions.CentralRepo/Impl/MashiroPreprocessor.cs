namespace IceCraft.Extensions.CentralRepo.Impl;

using IceCraft.Api.Installation;
using IceCraft.Api.Package;
using IceCraft.Extensions.CentralRepo.Runtime;

public class MashiroPreprocessor : IArtefactPreprocessor
{
    private readonly MashiroStatePool _statePool;

    public MashiroPreprocessor(MashiroStatePool statePool)
    {
        _statePool = statePool;
    }
    
    public async Task Preprocess(string tempExpandDir, string installDir, PackageMeta meta)
    {
        var state = await _statePool.GetAsync(meta);
        state.RunMetadata();
        if (state.PreprocessPackageDelegate == null)
        {
            throw new InvalidOperationException($"No preprocessor registered for package {meta.Id} ({meta.Version})");
        }
        
        await state.PreprocessPackageDelegate(tempExpandDir, installDir);
    }
}
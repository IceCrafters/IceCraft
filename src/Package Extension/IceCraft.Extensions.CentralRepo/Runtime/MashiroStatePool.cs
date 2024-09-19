namespace IceCraft.Extensions.CentralRepo.Runtime;

using IceCraft.Api.Package;
using IceCraft.Extensions.CentralRepo.Network;

public class MashiroStatePool
{
    private readonly RemoteRepositoryManager _remoteManager;
    private readonly Dictionary<PackageMeta, MashiroState> _mashiroStates = new();
    private readonly MashiroRuntime _runtime;

    public MashiroStatePool(RemoteRepositoryManager remoteManager, MashiroRuntime runtime)
    {
        _remoteManager = remoteManager;
        _runtime = runtime;
    }
    
    public async Task<MashiroState> GetAsync(PackageMeta packageMeta)
    {
        if (_mashiroStates.TryGetValue(packageMeta, out var value)) return value;
        
        var result = await LoadLocalStateAsync(packageMeta);
        _mashiroStates.Add(packageMeta, result);
        return result;
    }

    private async Task<MashiroState> LoadLocalStateAsync(PackageMeta packageMeta)
    {
        var fileName = $"{packageMeta.Id}-{packageMeta.Version}.js";
        if (packageMeta.AdditionalMetadata != null
            && packageMeta.AdditionalMetadata.TryGetValue("FileName", out var realFileName)
            && realFileName != null)
        {
            fileName = $"{realFileName}.js";
        }
        
        var path = Path.Combine(_remoteManager.LocalCachedRepoPath, "packages", fileName);
        Console.WriteLine(path);

        if (!File.Exists(path))
        {
            throw new InvalidOperationException("Package is non-existent on local cache");
        }
        
        return await _runtime.CreateStateAsync(path);
    }
}
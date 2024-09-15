namespace IceCraft.Extensions.CentralRepo.Runtime;

using IceCraft.Api.Package;
using IceCraft.Extensions.CentralRepo.Network;

public class MashiroStatePool
{
    private readonly RemoteRepositoryManager _remoteManager;
    private readonly Dictionary<PackageMeta, MashiroState> _mashiroStates = new();

    public MashiroStatePool(RemoteRepositoryManager remoteManager)
    {
        _remoteManager = remoteManager;
    }
    
    public MashiroState Get(PackageMeta packageMeta)
    {
        if (_mashiroStates.TryGetValue(packageMeta, out var value)) return value;
        
        var result = LoadLocalState(packageMeta);
        _mashiroStates.Add(packageMeta, result);
        return result;
    }

    private MashiroState LoadLocalState(PackageMeta packageMeta)
    {
        var path = Path.Combine(_remoteManager.LocalCachedRepoPath, $"{packageMeta.Id}-{packageMeta.Version}.js");

        if (!File.Exists(path))
        {
            throw new InvalidOperationException("Package is non-existent on local cache");
        }
        
        return MashiroRuntime.CreateState(path);
    }
}
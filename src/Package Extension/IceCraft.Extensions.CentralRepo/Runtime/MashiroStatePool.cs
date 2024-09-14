namespace IceCraft.Extensions.CentralRepo.Runtime;

using IceCraft.Api.Package;

public class MashiroStatePool
{
    private readonly Dictionary<PackageMeta, MashiroState> _mashiroStates = new();

    public MashiroState Get(PackageMeta packageMeta)
    {
        if (!_mashiroStates.TryGetValue(packageMeta, out var value))
        {
            // Get mashiro script from local cache
            throw new NotImplementedException();
        }
        
        return value;
    }
}
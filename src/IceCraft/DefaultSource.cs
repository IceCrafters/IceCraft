namespace IceCraft;

using System.Collections.Generic;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Repositories.Adoptium;

using SourceFactoryPair = KeyValuePair<string, Func<IServiceProvider, 
    IceCraft.Core.Archive.Providers.IRepositorySource>>;

public class DefaultSource : IRepositoryDefaultsSupplier
{
    public IEnumerable<SourceFactoryPair> GetDefaultSources()
    {
        // TODO make this extensible.
        return
        [
            new SourceFactoryPair(
                "adoptium", 
                provider => new AdoptiumRepositorySource(provider))
        ];
    }
}

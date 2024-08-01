namespace IceCraft;

using System;
using System.Collections.Generic;
using IceCraft.Core.Archive.Providers;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Repositories.Adoptium;

public class DefaultSource : IRepositoryDefaultsSupplier
{
    public IEnumerable<KeyValuePair<string, Func<IServiceProvider, IRepositorySource>>> GetDefaultSources()
    {
        // TODO make this extensible.
        return
        [
            new("adoptium", provider => new AdoptiumRepositorySource(provider))
        ];
    }
}

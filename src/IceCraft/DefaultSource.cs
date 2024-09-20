// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft;

using System.Collections.Generic;
using IceCraft.Api.Archive.Repositories;
using SourceFactoryPair = KeyValuePair<string, Func<IServiceProvider, 
    Api.Archive.Repositories.IRepositorySource>>;

public class DefaultSource : IRepositoryDefaultsSupplier
{
    public IEnumerable<SourceFactoryPair> GetDefaultSources()
    {
        // TODO make this extensible.
        return
        [
            // new SourceFactoryPair(
            //     "adoptium", 
            //     provider => new AdoptiumRepositorySource(provider))
        ];
    }
}

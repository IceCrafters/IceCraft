// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Repositories.Adoptium;

using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Installation;
using Microsoft.Extensions.DependencyInjection;

public static class AdoptiumExtensions
{
    private class SourceFactory : IRepositorySourceFactory
    {
        public IRepositorySource CreateRepositorySource(IServiceProvider provider, out string name)
        {
            name = "adoptium";
            return new AdoptiumRepositorySource(provider);
        }
    }

    public static IServiceCollection AddAdoptiumSource(this IServiceCollection collection)
    {
        return collection.AddKeyedTransient<IRepositorySourceFactory, SourceFactory>(null)
            .AddKeyedSingleton<IPackageInstaller, AdoptiumInstaller>("adoptium")
            .AddKeyedSingleton<IPackageConfigurator, AdoptiumConfigurator>("adoptium");
    }
}

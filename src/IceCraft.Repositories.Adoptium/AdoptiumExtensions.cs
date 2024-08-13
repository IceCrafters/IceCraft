namespace IceCraft.Repositories.Adoptium;

using IceCraft.Core.Archive.Providers;
using IceCraft.Core.Installation;
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
        return collection.AddKeyedTransient<IRepositorySourceFactory, SourceFactory>("adoptium")
            .AddKeyedSingleton<IPackageInstaller, AdoptiumInstaller>("adoptium");
    }
}

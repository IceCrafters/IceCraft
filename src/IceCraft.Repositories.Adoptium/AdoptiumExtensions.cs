namespace IceCraft.Repositories.Adoptium;

using IceCraft.Core.Archive.Providers;
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

    public static void AddAdoptiumSource(this IServiceCollection collection)
    {
        collection.AddKeyedTransient<IRepositorySourceFactory, SourceFactory>("adoptium");
    }
}

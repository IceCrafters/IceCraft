namespace IceCraft.Developer;

using System;
using System.Threading.Tasks;
using IceCraft.Core.Archive;
using IceCraft.Core.Archive.Providers;
using IceCraft.Core.Installation;
using Microsoft.Extensions.DependencyInjection;

public class DummyRepositorySource : IRepositorySource
{
    public Task<IRepository?> CreateRepositoryAsync()
    {
        return Task.FromResult<IRepository?>(new DummyRepository());
    }

    public Task RefreshAsync()
    {
        return Task.CompletedTask;
    }

    private class Factory : IRepositorySourceFactory
    {
        public IRepositorySource CreateRepositorySource(IServiceProvider provider, out string name)
        {
            name = "dummy-test";
            return new DummyRepositorySource();
        }
    }

    public static IServiceCollection AddDummyRepositorySource(IServiceCollection collection)
    {
        return collection.AddKeyedSingleton<IRepositorySourceFactory, Factory>(null)
            .AddKeyedSingleton<IPackageConfigurator, DummyPackageConfigurator>("dummy-test")
            .AddKeyedSingleton<IPackageInstaller, DummyPackageInstaller>("dummy-test");
    }
}

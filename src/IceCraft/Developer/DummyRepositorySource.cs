// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Developer;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Installation;
using Microsoft.Extensions.DependencyInjection;

public class DummyRepositorySource : IRepositorySource
{
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

    public IEnumerable<RepositoryInfo> CreateRepositories()
    {
        yield return new RepositoryInfo("dummy-test", new DummyRepository());
    }

    internal static void AddDummyRepositorySource(ContainerBuilder builder)
    {
        builder.RegisterType<Factory>().As<IRepositorySourceFactory>();
        builder.RegisterType<DummyPackageConfigurator>()
            .As<IPackageConfigurator>()
            .Keyed<IPackageConfigurator>("dummy-test");
        builder.RegisterType<DummyPackageInstaller>()
            .As<IPackageInstaller>()
            .Keyed<IPackageInstaller>("dummy-test");
    }
}

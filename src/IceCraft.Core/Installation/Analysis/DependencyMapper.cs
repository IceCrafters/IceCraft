// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Installation.Analysis;

using System.Collections.Generic;
using IceCraft.Api.Caching;
using IceCraft.Api.Client;
using IceCraft.Api.Installation.Database;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;
using IceCraft.Core.Archive.Dependency;
using IceCraft.Core.Caching;
using IceCraft.Core.Serialization;
using Microsoft.Extensions.DependencyInjection;

public class DependencyMapper : IDependencyMapper, ICacheClearable
{
    private static readonly Guid CacheGuid = new("478DA958-DADF-4789-B05A-B63A095D97D6");
    private const string CacheObjectName = "dependencyMap";

    private readonly IServiceProvider _serviceProvider;
    private readonly IOutputAdapter _output;
    private readonly ICacheStorage _cacheStorage;

    public DependencyMapper(IServiceProvider serviceProvider,
        IFrontendApp frontendApp,
        ICacheManager cacheManager)
    {
        _serviceProvider = serviceProvider;
        _output = frontendApp.Output;
        _cacheStorage = cacheManager.GetStorage(CacheGuid);
    }

    public async Task<DependencyMap> MapDependencies()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var readHandle = await scope.ServiceProvider.GetRequiredService<ILocalDatabaseReadAccess>()
            .GetReadHandle();
        
        var map = new DependencyMap(readHandle.Count);

        await Task.Run(async () =>
        {
            foreach (var package in readHandle.EnumeratePackages())
            {
                await ProcessPackage(package, map, readHandle);
            }
        });

        return map;
    }

    public async Task<DependencyMap> MapDependenciesCached()
    {
        return await _cacheStorage.RollJsonAsync(CacheObjectName,
            MapDependencies,
            IceCraftCoreContext.Default.DependencyMap);
    }

    private async ValueTask ProcessPackage(PackageMeta package,
        DependencyMap map,
        ILocalDatabaseReadHandle readHandle)
    {
        var entry = map.GetEntry(package);

        if (package.Dependencies == null)
        {
            return;
        }

        foreach (var dependency in package.Dependencies)
        {
            var best = await DependencyResolver.SelectBestPackageDependencyOrDefault(readHandle.EnumeratePackages(),
                dependency);

            if (best == null)
            {
                entry.HasUnsatisifiedDependencies = true;
                continue;
            }

            var bestEntry = map.GetEntry(best);

            entry.Dependencies.Add(new PackageReference(best.Id,
                best.Version));
            bestEntry.Dependents.Add(new PackageReference(package.Id,
                package.Version));
        }

        if (entry.HasUnsatisifiedDependencies)
        {
            _output.Warning("Package '{0}' ({1}) has UNSATISFIED dependencies", package.Id, package.Version);
        }
    }
    
    public void ClearCache()
    {
        _cacheStorage.DeleteObject(CacheObjectName);
    }

    public async IAsyncEnumerable<DependencyReference> MapUnmetDependencies()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var readHandle = await scope.ServiceProvider.GetRequiredService<ILocalDatabaseReadAccess>()
            .GetReadHandle();
        
        var map = new DependencyMap(readHandle.Count);

        foreach (var package in readHandle.EnumeratePackages())
        {
            map.GetEntry(package);

            if (package.Dependencies == null)
            {
                continue;
            }

            foreach (var dependency in package.Dependencies)
            {
                var best = await DependencyResolver.SelectBestPackageDependencyOrDefault(readHandle.EnumeratePackages(),
                    dependency);

                if (best == null)
                {
                    yield return dependency;
                }
            }
        }
    }

    public async IAsyncEnumerable<PackageMeta> EnumerateUnsatisifiedPackages()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var readHandle = await scope.ServiceProvider.GetRequiredService<ILocalDatabaseReadAccess>()
            .GetReadHandle();
        var map = new DependencyMap(readHandle.Count);

        foreach (var package in readHandle.EnumeratePackages())
        {
            map.GetEntry(package);

            if (package.Dependencies == null)
            {
                continue;
            }

            var noBest = false;
            foreach (var dependency in package.Dependencies)
            {
                var best = await DependencyResolver.SelectBestPackageDependencyOrDefault(readHandle.EnumeratePackages(),
                    dependency);

                if (best == null)
                {
                    noBest = true;
                    yield return package;
                    break;
                }
            }

            if (noBest)
            {
            }
        }
    }
}
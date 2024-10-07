// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Archive.Indexing;

using System.Threading.Tasks;
using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Archive.Indexing;
using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Caching;
using IceCraft.Api.Client;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Caching;
using IceCraft.Core.Serialization;
using Microsoft.Extensions.Logging;

public class CachedIndexer : IPackageIndexer, ICacheClearable
{
    private static readonly Guid CacheStorageId = new("5ce7b9d1-0aea-4aa1-bb9d-e713c457c632");
    private const string PackageIndexStorage = "pkgIndex_v0_1";
    
    private readonly ICacheStorage _cacheStorage;

    private readonly IOutputAdapter _output;

    public CachedIndexer(ICacheManager cacheManager,
        IFrontendApp frontendApp)
    {
        _cacheStorage = cacheManager.GetStorage(CacheStorageId);
        _output = frontendApp.Output;
    }

    public async Task<PackageIndex> IndexAsync(IRepositorySourceManager manager,
        CancellationToken cancellationToken = default)
    {
        var dict = await _cacheStorage.RollJsonAsync(PackageIndexStorage, 
            async () => await GenerateNewIndex(manager, cancellationToken),
            IceCraftCoreContext.Default.BasePackageIndex_v0_1);

        return new PackageIndex(dict);
    }

    private async Task<Dictionary<string, CachedPackageSeriesInfo>> GenerateNewIndex(IRepositorySourceManager manager, 
        CancellationToken cancellationToken = default)
    {
        var index = new Dictionary<string, CachedPackageSeriesInfo>(manager.Count);
        var repoCount = 0;
        
        await foreach (var (key, repo) in manager.EnumerateRepositoriesAsync())
        {
            repoCount++;
            cancellationToken.ThrowIfCancellationRequested();

            index.EnsureCapacity(index.Count + repo.GetExpectedSeriesCount());

            _output.Tagged("SCAN", key);

            foreach (var series in repo.EnumerateSeries())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var expectedCount = await series.GetExpectedPackageCountAsync();

                _output.Verbose("Indexing series {0} with ~{1} packages", series.Name, expectedCount);

                // Creates this whole dictionary of version information.
                var versions = new Dictionary<string, CachedPackageInfo>(
                    expectedCount);

                if (series is AsyncPackageSeries asyncSeries)
                {
                    await IterateSeriesInternalAsync(asyncSeries, versions, cancellationToken);
                }
                else
                {
                    // This can well be IO bound
                    // TODO make this not IO bound
                    IterateSeriesInternal(series, versions, cancellationToken);
                }

                index.Add(series.Name, new CachedPackageSeriesInfo
                {
                    Name = series.Name,
                    Versions = versions,
                });
            }
        }

        _output.Log("Total {0} repositories indexed", repoCount);

        return index;
    }

    public void ClearCache()
    {
        _cacheStorage.DeleteObject(PackageIndexStorage);
    }

    private void IterateSeriesInternal(IPackageSeries series, 
        Dictionary<string, CachedPackageInfo> versions,
        CancellationToken cancellationToken = default)
    {
        foreach (var package in series.EnumeratePackages(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Go through everything.
            var pkgMeta = package.GetMeta();
            _output.Verbose("Indexing version {0}", pkgMeta.Version);

            IArtefactDefinition artefactDef;
            IEnumerable<ArtefactMirrorInfo>? mirrors;

            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                artefactDef = package.GetArtefact();
                mirrors = package.GetMirrors();
            }
            catch (Exception ex)
            {
                _output.Warning(ex, "Failed to index package {0} {1}", pkgMeta.Id, pkgMeta.Version);
                continue;
            }

            versions.Add(pkgMeta.Version.ToString(), new CachedPackageInfo
            {
                Metadata = pkgMeta,
                Artefact = artefactDef,
                Mirrors = mirrors
            });
        }
    }
    
    private async Task IterateSeriesInternalAsync(AsyncPackageSeries series,
        Dictionary<string, CachedPackageInfo> versions,
        CancellationToken cancellationToken = default)
    {
        await foreach (var package in series.EnumeratePackagesAsync(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Go through everything.
            var pkgMeta = package.GetMeta();
            _output.Verbose("Indexing version {0}", pkgMeta.Version);

            IArtefactDefinition artefactDef;
            IEnumerable<ArtefactMirrorInfo>? mirrors;

            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                artefactDef = package.GetArtefact();
                mirrors = package.GetMirrors();
            }
            catch (Exception ex)
            {
                _output.Warning(ex, "Failed to index package {0} {1}", pkgMeta.Id, pkgMeta.Version);
                continue;
            }

            versions.Add(pkgMeta.Version.ToString(), new CachedPackageInfo
            {
                Metadata = pkgMeta,
                Artefact = artefactDef,
                Mirrors = mirrors
            });
        }
    }
}

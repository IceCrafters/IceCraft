namespace IceCraft.Core.Archive.Indexing;

using System.Threading.Tasks;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Caching;
using IceCraft.Core.Serialization;
using Microsoft.Extensions.Logging;

public class CachedIndexer : IPackageIndexer, ICacheClearable
{
    private static readonly Guid CacheStorageId = new("5ce7b9d1-0aea-4aa1-bb9d-e713c457c632");
    private const string PackageIndexStorage = "pkgIndex_v0_1";
    
    private readonly ICacheStorage _cacheStorage;

    private readonly ILogger<CachedIndexer> _logger;

    public CachedIndexer(ICacheManager cacheManager,
        ILogger<CachedIndexer> logger)
    {
        _cacheStorage = cacheManager.GetStorage(CacheStorageId);
        _logger = logger;
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
        var repos = await manager.GetRepositoriesAsync();
        var repoCount = 0;
        
        foreach (var repo in repos)
        {
            repoCount++;
            cancellationToken.ThrowIfCancellationRequested();

            index.EnsureCapacity(index.Count + repo.GetExpectedSeriesCount());

            foreach (var series in repo.EnumerateSeries())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var expectedCount = await series.GetExpectedPackageCountAsync();

                _logger.LogInformation("Indexing series {Name} with {ExpectedCount} packages", series.Name, expectedCount);

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

        _logger.LogInformation("{RepoCount} repositories indexed", repoCount);

        return index;
    }

    public void ClearCache()
    {
        _cacheStorage.DeleteObject(PackageIndexStorage);
    }

    private void IterateSeriesInternal(IPackageSeries series,
        IDictionary<string, CachedPackageInfo> versions,
        CancellationToken cancellationToken = default)
    {
        foreach (var package in series.EnumeratePackages(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Go through everything.
            var pkgMeta = package.GetMeta();
            _logger.LogTrace("Indexing version {Version}", pkgMeta.Version);

            RemoteArtefact remoteArtefact;
            IEnumerable<ArtefactMirrorInfo>? mirrors;

            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                remoteArtefact = package.GetArtefact();
                mirrors = package.GetMirrors();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to index package {Id} {Version}", pkgMeta.Id, pkgMeta.Version);
                continue;
            }

            versions.Add(pkgMeta.Version.ToString(), new CachedPackageInfo
            {
                Metadata = pkgMeta,
                Artefact = remoteArtefact,
                Mirrors = mirrors
            });
        }
    }
    
    private async Task IterateSeriesInternalAsync(AsyncPackageSeries series,
        IDictionary<string, CachedPackageInfo> versions,
        CancellationToken cancellationToken = default)
    {
        await foreach (var package in series.EnumeratePackagesAsync(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Go through everything.
            var pkgMeta = package.GetMeta();
            _logger.LogTrace("Indexing version {Version}", pkgMeta.Version);

            RemoteArtefact remoteArtefact;
            IEnumerable<ArtefactMirrorInfo>? mirrors;

            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                remoteArtefact = package.GetArtefact();
                mirrors = package.GetMirrors();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to index package {Id} {Version}", pkgMeta.Id, pkgMeta.Version);
                continue;
            }

            versions.Add(pkgMeta.Version.ToString(), new CachedPackageInfo
            {
                Metadata = pkgMeta,
                Artefact = remoteArtefact,
                Mirrors = mirrors
            });
        }
    }
}

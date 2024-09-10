namespace IceCraft.Api.Archive.Indexing;

using IceCraft.Api.Archive.Repositories;

public interface IPackageIndexer
{
    Task<PackageIndex> IndexAsync(IRepositorySourceManager manager, CancellationToken cancellationToken = default);
}

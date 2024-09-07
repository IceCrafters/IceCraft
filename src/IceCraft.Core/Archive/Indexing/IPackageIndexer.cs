namespace IceCraft.Core.Archive.Indexing;

using IceCraft.Core.Archive.Repositories;

public interface IPackageIndexer
{
    Task<PackageIndex> IndexAsync(IRepositorySourceManager manager, CancellationToken cancellationToken = default);
}

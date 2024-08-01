namespace IceCraft.Core.Archive.Indexing;

public interface IPackageIndexer
{
    Task<PackageIndex> IndexAsync(IRepositorySourceManager manager);
}

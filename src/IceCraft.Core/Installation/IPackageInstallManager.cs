namespace IceCraft.Core.Installation;

using IceCraft.Core.Archive.Indexing;

public interface IPackageInstallManager
{
    Task InstallAsync(CachedPackageInfo packageInfo);   
}

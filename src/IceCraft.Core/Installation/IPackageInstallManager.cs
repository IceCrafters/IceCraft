namespace IceCraft.Core.Installation;

using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Packaging;

public interface IPackageInstallManager
{
    Task InstallAsync(CachedPackageInfo packageInfo);
    Task InstallAsync(PackageMeta meta, string artefactPath);
    Task<string> GetInstalledPackageDirectoryAsync(PackageMeta meta);
}

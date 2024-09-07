namespace IceCraft.Core.Installation;

using IceCraft.Core.Archive.Packaging;

public interface IPackageConfigurator
{
    Task ConfigurePackageAsync(string installDir, PackageMeta meta);
    Task UnconfigurePackageAsync(string installDir, PackageMeta meta);
}

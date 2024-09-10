namespace IceCraft.Api.Installation;

using IceCraft.Api.Package;

public interface IPackageConfigurator
{
    Task ConfigurePackageAsync(string installDir, PackageMeta meta);
    Task UnconfigurePackageAsync(string installDir, PackageMeta meta);
}

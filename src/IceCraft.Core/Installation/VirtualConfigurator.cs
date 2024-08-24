namespace IceCraft.Core.Installation;

using IceCraft.Core.Archive.Packaging;

public class VirtualConfigurator : IPackageConfigurator
{
    public Task ConfigurePackageAsync(string installDir, PackageMeta meta)
    {
        return Task.CompletedTask;
    }

    public Task UnconfigurePackageAsync(string installDir, PackageMeta meta)
    {
        return Task.CompletedTask;
    }
}
namespace IceCraft.Developer;

using System.Threading.Tasks;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Installation;

public class DummyPackageConfigurator : IPackageConfigurator
{
    public async Task ConfigurePackageAsync(string installDir, PackageMeta meta)
    {
        await File.WriteAllTextAsync(Path.Combine(installDir, "configured"), "Dummy Configuration Package");
    }

    public Task UnconfigurePackageAsync(string installDir, PackageMeta meta)
    {
        File.Delete(Path.Combine(installDir, "configured"));
        return Task.CompletedTask;
    }
}

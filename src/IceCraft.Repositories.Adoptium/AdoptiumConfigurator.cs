namespace IceCraft.Repositories.Adoptium;

using System.Threading.Tasks;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Installation;
using IceCraft.Core.Installation.Execution;

public class AdoptiumConfigurator : IPackageConfigurator
{
    private readonly IExecutableManager _executableManager;

    public AdoptiumConfigurator(IExecutableManager executableManager)
    {
        _executableManager = executableManager;
    }

    public async Task ConfigurePackageAsync(string installDir, PackageMeta meta)
    {
        await _executableManager.RegisterAsync(meta, "java", "bin/java");
        await _executableManager.RegisterAsync(meta, "javaw", "bin/javaw");
    }

    public async Task UnconfigurePackageAsync(string installDir, PackageMeta meta)
    {
        await _executableManager.UnregisterAsync(meta, "java");
        await _executableManager.UnregisterAsync(meta, "javaw");
    }
}

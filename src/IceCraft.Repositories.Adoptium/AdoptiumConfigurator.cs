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
        await _executableManager.LinkExecutableAsync(meta, "java", "bin/java");
        await _executableManager.LinkExecutableAsync(meta, "javaw", "bin/javaw");
    }
}

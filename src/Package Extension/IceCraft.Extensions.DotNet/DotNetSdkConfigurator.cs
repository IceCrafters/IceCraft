namespace IceCraft.Extensions.DotNet;

using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Installation;
using IceCraft.Core.Installation.Analysis;
using IceCraft.Core.Installation.Execution;
using IceCraft.Core.Platform;
using IceCraft.Extensions.DotNet.Archive;
using Semver;

public class DotNetSdkConfigurator : IPackageConfigurator
{
    private readonly IPackageInstallManager _installManager;
    private readonly IExecutableManager _executableManager;
    private readonly IEnvironmentManager _environmentManager;

    public DotNetSdkConfigurator(IPackageInstallManager installManager, 
        IExecutableManager executableManager, 
        IEnvironmentManager environmentManager)
    {
        _installManager = installManager;
        _executableManager = executableManager;
        _environmentManager = environmentManager;
    }
    
    public async Task ConfigurePackageAsync(string installDir, PackageMeta meta)
    {
        if (meta.AdditionalMetadata?.TryGetValue(DotNetSdkPackage.MetadataRuntimeVersion,
                out var runtimeVersion) == true)
        {
            var virtualMeta = new PackageMeta
            {
                Id = $"dotnet-runtime-{meta.Version.Major}.0",
                Version = SemVersion.Parse(runtimeVersion, SemVersionStyles.Strict),
                ReleaseDate = meta.ReleaseDate,
                PluginInfo = new PackagePluginInfo("virtual", "virtual")
            };
            
            await _installManager.RegisterVirtualPackageAsync(virtualMeta,
                new PackageReference(meta.Id, meta.Version));
        }

        // Registering executable makes Rider and potentially other tools fail to locate .NET SDK.
        // Instead, we register PATH directly.
        // await _executableManager.RegisterAsync(meta, "dotnet", "dotnet")
        
        _environmentManager.AddUserGlobalPath(installDir);
        _environmentManager.AddUserVariable("DOTNET_ROOT", installDir);
    }

    public async Task UnconfigurePackageAsync(string installDir, PackageMeta meta)
    {
        // Clean any leftover executables from the old logic.
        await _executableManager.UnregisterAsync(meta, "dotnet");
        
        _environmentManager.RemoveUserGlobalPath(installDir);
        _environmentManager.RemoveUserVariable("DOTNET_ROOT");
    }
}
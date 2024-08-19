using System.IO.Abstractions;
using IceCraft;
using IceCraft.Core;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Caching;
using IceCraft.Core.Configuration;
using IceCraft.Core.Platform;
using IceCraft.Developer;
using IceCraft.Frontend;
using IceCraft.Frontend.Commands;
using IceCraft.Repositories.Adoptium;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

IceCraftApp.Initialize();
var appServices = new ServiceCollection();
appServices
    // Application
    .AddSingleton<IManagerConfiguration, DotNetConfigServiceImpl>()
    .AddSingleton<IFrontendApp, IceCraftApp>()
    .AddSingleton<ICacheManager, FileSystemCacheManager>()
    .AddSingleton<IRepositoryDefaultsSupplier, DefaultSource>()
    .AddSingleton<IFileSystem, FileSystem>()
    .AddLogging(configure => configure.AddSerilog())
    // Core
    .AddIceCraftDefaults()
    // Sources
    .AddAdoptiumSource();

#if DEBUG
DummyRepositorySource.AddDummyRepositorySource(appServices);
#endif

var registrar = new TypeRegistrar(appServices);

// Initialize command line

var cmdApp = new CommandApp(registrar);

cmdApp.Configure(root =>
{
    root.SetApplicationName("IceCraft");
    root.SetApplicationVersion(IceCraftApp.ProductVersion);

    root.AddCommand<UpdateCommand>("update")
        .WithDescription("Regenerates package cache and refs");

    root.AddCommand<InfoCommand>("info")
        .WithDescription("Shows various metadata for a package series");

    root.AddBranch<SourceSwitchCommand.Settings>("source", source =>
    {
        source.AddCommand<SourceSwitchCommand.EnableCommand>("enable");
        source.AddCommand<SourceSwitchCommand.DisableCommand>("disable");
    });

    root.AddBranch<BaseSettings>("cache", cache =>
    {
        cache.AddCommand<ClearCacheCommand>("clear");
        cache.AddCommand<MaintainCacheCommand>("maintain");
    });

    root.AddBranch<BaseSettings>("package", package =>
    {
        package.AddCommand<PackageListCommand>("list");
    });

    root.AddCommand<DownloadCommand>("download");
    root.AddCommand<InstallCommand>("install");
    
    root.AddCommand<MirrorGetBestCommand>("best-mirror")
        .WithDescription("Tests for the best mirror for a given package");

    root.SetExceptionHandler((ex, _) =>
    {
        if (ex is CommandRuntimeException cex)
        {
            if (cex.InnerException != null)
            {
                Log.Fatal(ex, "Failed to set up application");
                return;
            }

            AnsiConsole.MarkupLineInterpolated($"[red][bold]IceCraft: {cex.Message}[/][/]");
            return;
        }

        Log.Fatal(ex, "Unknown error occurred");
    });

    root.SetInterceptor(new LogInterceptor());
});

await cmdApp.RunAsync(args);
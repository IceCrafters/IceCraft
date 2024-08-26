using System.Diagnostics;
using System.IO.Abstractions;
using IceCraft;
using IceCraft.Core;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Caching;
using IceCraft.Core.Configuration;
using IceCraft.Core.Platform;
using IceCraft.Core.Util;
using IceCraft.Developer;
using IceCraft.Extensions.DotNet;
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
    .AddAdoptiumSource()
    .AddDotNetExtension();

#if DEBUG
if (!Debugger.IsAttached)
{
    AnsiConsole.WriteLine("Attach a debugger, and then PRESS ANY KEY...");
    Console.ReadKey(true);
}

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

    root.AddCommand<InitializeCommand>("init");

    root.AddBranch<BaseSettings>("source", source =>
    {
        source.AddCommand<SourceSwitchCommand.EnableCommand>("enable");
        source.AddCommand<SourceSwitchCommand.DisableCommand>("disable");
    });

    root.AddBranch<BaseSettings>("cache", cache =>
    {
        cache.AddCommand<ClearCacheCommand>("clear");
        cache.AddCommand<MaintainCacheCommand>("maintain");
        cache.AddCommand<RegenerateDependMapCommand>("regen-dependmap");
    });

    root.AddBranch<BaseSettings>("package", package =>
    {
        package.AddCommand<PackageListCommand>("list");
        package.AddCommand<PackageReconfigureCommand>("reconfigure");
        root.AddCommand<MirrorGetBestCommand>("best-mirror")
            .WithDescription("Tests for the best mirror for a given package");
    });

    root.AddCommand<DownloadCommand>("download");
    root.AddCommand<InstallCommand>("install");
    root.AddCommand<UninstallCommand>("remove");

    root.SetExceptionHandler((ex, _) =>
    {
        switch (ex)
        {
            case CommandRuntimeException { InnerException: not null }:
                Log.Fatal(ex, "Failed to set up application");
                return;
            case CommandRuntimeException cex:
                AnsiConsole.MarkupLineInterpolated($"[red][bold]IceCraft: {cex.Message}[/][/]");
                return;
            case OperationCanceledException:
                return;
            // Known exceptions are errors that are expected to occur unlike Unknown error which
            // something is terribly wrong.
            case KnownException kex:
                AnsiConsole.MarkupLineInterpolated($"[red][bold]IceCraft: {kex.Message}[/][/]");
                Log.Verbose(kex, "Details:");
                return;
            default:
                Log.Fatal(ex, "Unknown error occurred");
                break;
        }
    });

    root.SetInterceptor(new StandardInterceptor());
});

await cmdApp.RunAsync(args);
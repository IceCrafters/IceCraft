using IceCraft;
using IceCraft.Core;
using IceCraft.Core.Archive.Checksums;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Providers;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Caching;
using IceCraft.Core.Configuration;
using IceCraft.Frontend;
using IceCraft.Frontend.Commands;
using IceCraft.Repositories.Adoptium;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Internal.Configuration;

IceCraftApp.Initialize();
var appServices = new ServiceCollection();
appServices.AddSingleton<IManagerConfiguration, DNConfigImpl>()
    .AddSingleton<ICacheManager, FileSystemCacheManager>()
    .AddSingleton<IRepositoryDefaultsSupplier, DefaultSource>()
    .AddSingleton<IRepositorySourceManager, RepositoryManager>()
    .AddSingleton<IChecksumRunner, DependencyChecksumRunner>()
    .AddSingleton<IPackageIndexer, CachedIndexer>()
    // Hash validators
    .AddKeyedSingleton<IChecksumValidator, Sha256ChecksumValidator>("sha256")
    .AddLogging(configure => configure.AddSerilog());

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

    root.SetExceptionHandler((ex, resolver) =>
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
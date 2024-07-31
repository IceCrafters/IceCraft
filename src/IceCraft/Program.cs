using IceCraft;
using IceCraft.Core;
using IceCraft.Core.Archive.Providers;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Caching;
using IceCraft.Core.Configuration;
using IceCraft.Frontend;
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
    .AddSingleton<IRepositorySourceManager, RepositoryManager>()
    // Sources
    .AddKeyedSingleton<IRepositorySource, AdoptiumRepositoryProvider>("adoptium")
    .AddLogging(configure => configure.AddSerilog());

var registrar = new TypeRegistrar(appServices);

// Initialize command line

var cmdApp = new CommandApp(registrar);

cmdApp.Configure(root =>
{
    root.SetApplicationVersion(IceCraftApp.ProductVersion);

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
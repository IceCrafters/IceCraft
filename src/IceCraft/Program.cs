using System.Diagnostics;
using IceCraft;
using IceCraft.Core;
using IceCraft.Core.Archive.Providers;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Caching;
using IceCraft.Core.Configuration;
using IceCraft.Frontend;
using IceCraft.Repositories.Adoptium;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

IceCraftApp.Initialize();
var appServices = new ServiceCollection();
appServices.AddSingleton<IManagerConfiguration, DNConfigImpl>()
    .AddSingleton<ICacheManager, FileSystemCacheManager>()
    .AddSingleton<IRepositorySourceManager, RepositoryManager>()
    // Sources
    .AddKeyedSingleton<IRepositorySource, AdoptiumRepositoryProvider>("adoptium");

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

    // root.SetInterceptor(interceptor);
});

// // Final initialization
// var provider = registrar.Provider;
// var repoMan = provider.GetRequiredService<IRepositorySourceManager>();
// repoMan.RegisterSourceAsService("adoptium");

await cmdApp.RunAsync(args);
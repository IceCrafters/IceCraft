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
    .AddKeyedSingleton<IRepositorySource, AdoptiumRepositoryProvider>("adoptium");

var provider = appServices.BuildServiceProvider();
var repoMan = provider.GetRequiredService<IRepositorySourceManager>();

repoMan.RegisterSourceAsService("adoptium");

var stopwatch = Stopwatch.StartNew();

var repos = await repoMan.GetRepositories();
stopwatch.Stop();
Console.WriteLine("Acquiring repositories took {0}ms", stopwatch.ElapsedMilliseconds);

// Initialize command line

var registrar = new TypeRegistrar(appServices);

var cmdApp = new CommandApp(registrar);

cmdApp.Configure(root =>
{
    root.SetApplicationVersion(IceCraftApp.ProductVersion);

    root.AddBranch<SourceSwitchCommand.Settings>("source", source =>
    {
        source.AddCommand<SourceSwitchCommand.EnableCommand>("enable");
        source.AddCommand<SourceSwitchCommand.DisableCommand>("disable");
    });
});

await cmdApp.RunAsync(args);
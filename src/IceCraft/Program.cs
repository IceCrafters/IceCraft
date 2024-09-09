using System.CommandLine;
using System.CommandLine.Builder;
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
using IceCraft.Extensions.CentralRepo;
using IceCraft.Extensions.DotNet;
using IceCraft.Frontend;
using IceCraft.Frontend.Cli;
using IceCraft.Repositories.Adoptium;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;

#if LEGACY_INTERFACE
using IceCraft.Core.Util;
using IceCraft.Frontend.Commands;
using Spectre.Console.Cli;
#endif

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
    .AddDotNetExtension()
    .AddCsrExtension();

#if DEBUG
if (!Debugger.IsAttached && args.Contains("--debug"))
{
    AnsiConsole.WriteLine($"{FrontendUtil.BaseName}: Attach a debugger, and then PRESS ANY KEY...");
    Console.ReadKey(true);
}

DummyRepositorySource.AddDummyRepositorySource(appServices);
#endif

// New interface with System.CommandLine

var serviceProvider = appServices.BuildServiceProvider();

var command = RootCommandFactory.CreateCommand(serviceProvider);

// Build middleware
var builder = new CommandLineBuilder(command);

// Verbose
builder.AddMiddleware(async (context, next) =>
{
    context.ConfigureVerbose();
    await next(context);
});

try
{
    return await command.InvokeAsync(args);
}
catch (KnownException e)
{
    AnsiConsole.MarkupLineInterpolated($"[red][bold]{FrontendUtil.BaseName}: {e.Message}[/][/]");
    Output.Shared.Verbose(e.StackTrace ?? "No stack trace available");
    return ExitCodes.GenericError;
}
catch (Exception e)
{
    Console.Error.WriteLine(e.ToString());
    return ExitCodes.GenericError;
}
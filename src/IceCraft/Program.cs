// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

using System.CommandLine;
using System.CommandLine.Builder;
using System.Diagnostics;
using System.IO.Abstractions;
using IceCraft;
using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Caching;
using IceCraft.Api.Client;
using IceCraft.Api.Exceptions;
using IceCraft.Core;
using IceCraft.Developer;
using IceCraft.Extensions.CentralRepo;
using IceCraft.Extensions.DotNet;
using IceCraft.Frontend;
using IceCraft.Frontend.Cli;
using IceCraft.Plugin;
using IceCraft.Repositories.Adoptium;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;

IceCraftApp.Initialize();
var appServices = new ServiceCollection();
appServices
    // Application
    .AddSingleton<IManagerConfiguration, DotNetConfigServiceImpl>()
    .AddSingleton<IFrontendApp, IceCraftApp>()
    .AddSingleton<ICacheManager, FileSystemCacheManager>()
    .AddSingleton<IRepositoryDefaultsSupplier, DefaultSource>()
    .AddSingleton<IFileSystem, FileSystem>()
    .AddSingleton<ICustomConfig, CustomConfigImpl>()
    .AddLogging(configure => configure.AddSerilog())
    // Core
    .AddIceCraftDefaults()
    // Sources
    .AddAdoptiumSource()
    .AddDotNetExtension();

var pluginManager = new PluginManager();
pluginManager.Add(new CsrPlugin());

pluginManager.InitializeAll(new ServiceRegistry(appServices));

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
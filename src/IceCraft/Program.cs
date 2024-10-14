// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO.Abstractions;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using DotNetConfig;
using IceCraft;
using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Caching;
using IceCraft.Api.Client;
using IceCraft.Api.Exceptions;
using IceCraft.Api.Installation;
using IceCraft.Api.Installation.Database;
using IceCraft.Core;
using IceCraft.Core.Installation.Storage;
using IceCraft.Developer;
using IceCraft.Extensions.CentralRepo;
using IceCraft.Extensions.DotNet;
using IceCraft.Frontend;
using IceCraft.Frontend.Cli;
using IceCraft.Frontend.Injection;
using IceCraft.Plugin;
using IceCraft.Repositories.Adoptium;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;

internal static class Program
{
    private static readonly IceCraftApp AppImpl = new();
    private static readonly Config ConfigInstance = Config.Build();

    private static async Task<int> Main(string[] args)
    {
        IceCraftApp.Initialize();
        var app = new IceCraftApp();
        var config = Config.Build();

        var dbFile = await app.ReadDatabase();

        var others = new ServiceCollection()
            .AddLogging(configure => configure.AddSerilog())
            .AddAdoptiumSource()
            .AddDotNetExtension();

        var pluginManager = new PluginManager();
        pluginManager.Add(new CsrPlugin());
        pluginManager.Add(new ClientPlugin());

        pluginManager.InitializeAll(others);

#if DEBUG
        if (!Debugger.IsAttached && args.Contains("--debug"))
        {
            AnsiConsole.WriteLine($"{Path.GetFileNameWithoutExtension(FrontendUtil.BaseName)}: Attach a debugger, and then PRESS ANY KEY...");
            Console.ReadKey(true);
        }
#endif

        // Register all services.
        var services = await BuildContainerAsync(others);
        var serviceProvider = new AutofacServiceProvider(services);

        var command = RootCommandFactory.CreateCommand(serviceProvider);

        // Build middleware
        var builder = new CommandLineBuilder(command);

        // Verbose
        builder.AddMiddleware(async (context, next) =>
        {
            context.ConfigureVerbose();
            await next(context);
        })
        .UseExceptionHandler((ex, context) =>
        {
            if (ex is KnownException known)
            {
                AnsiConsole.MarkupLineInterpolated($"[red][bold]{FrontendUtil.BaseName}: {ex.Message}[/][/]");
                Output.Shared.Verbose(ex.StackTrace ?? "No stack trace available");

                context.ExitCode = ExitCodes.GenericError;
            }
            else
            {
                Output.Shared.Error("Unknown error occurred!");
                Output.Error(ex);

                context.ExitCode = ExitCodes.GenericError;
            }
        })
        .UseHelp()
        .UseVersionOption();

        var parser = builder.Build();

        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            Output.Shared.Error("Unhandled error occurred!");
            Output.Shared.Error(args.ExceptionObject?.ToString() ?? "(no error information available)");
        };

        return await parser.InvokeAsync(args);
    }

    private static async Task<IContainer> BuildContainerAsync(IServiceCollection others)
    {
        var dbFile = await AppImpl.ReadDatabase();

        var builder = new ContainerBuilder();

        builder.RegisterInstance(AppImpl).As<IFrontendApp>().SingleInstance();
        builder.RegisterInstance(dbFile).As<DatabaseFile>().SingleInstance();
        builder.RegisterInstance(ConfigInstance).As<Config>().SingleInstance();

        builder.RegisterType<DotNetConfigServiceImpl>().As<IManagerConfiguration>().SingleInstance();
        builder.RegisterType<FileSystemCacheManager>().As<ICacheManager>().SingleInstance();
        builder.RegisterType<DefaultSource>().As<IRepositoryDefaultsSupplier>();
        builder.RegisterType<FileSystem>().As<IFileSystem>().SingleInstance();
        builder.RegisterType<LocalDatabaseAccessImpl>().As<ILocalDatabaseAccess>();
        builder.RegisterType<PackageSetupLifetimeImpl>().As<IPackageSetupLifetime>();

        builder.PopulateIceCraftCore();
        builder.Populate(others);

#if DEBUG
        DummyRepositorySource.AddDummyRepositorySource(builder);
#endif

        return builder.Build();
    }
}

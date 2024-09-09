namespace IceCraft.Frontend.Commands;

using System.CommandLine;
using System.Threading.Tasks;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Platform;
using IceCraft.Frontend.Cli;
using Serilog;
using Spectre.Console;

#if LEGACY_INTERFACE
using System.ComponentModel;
using Spectre.Console.Cli;
#endif

using CliCommand = System.CommandLine.Command;

#if LEGACY_INTERFACE
[Description("List all versions for a given package")]
#endif
public sealed class PackageListCommand
#if LEGACY_INTERFACE
    : AsyncCommand<PackageListCommand.Settings>
#endif
{
    private readonly IRepositorySourceManager _sourceManager;
    private readonly IPackageIndexer _indexer;
    private readonly IFrontendApp _frontend;

    public PackageListCommand(IRepositorySourceManager sourceManager,
        IPackageIndexer indexer,
        IFrontendApp frontend)
    {
        _sourceManager = sourceManager;
        _indexer = indexer;
        _frontend = frontend;
    }

    public CliCommand CreateCli()
    {
        var argPackage = new Argument<string>("package", "Package to list");
        var optPrerelease = new Option<bool>(["-P", "--include-prerelease"], "Include prerelease versions in output");

        var command = new CliCommand("list", "List package versions")
        {
            argPackage,
            optPrerelease
        };

        command.SetHandler(async context => context.ExitCode = await ExecuteInternalAsync(
            context.GetArgNotNull(argPackage),
            context.GetOpt(optPrerelease),
            context.GetCancellationToken()
        ));

        return command;
    }
    
    private async Task<int> ExecuteInternalAsync(string packageName, bool includePrerelease, CancellationToken cancellationToken)
    {
        var index = await _indexer.IndexAsync(_sourceManager, _frontend.GetCancellationToken());

        if (!index.TryGetValue(packageName, out var seriesInfo))
        {
            _frontend.Output.Error("Package {0} not found", packageName);
            return -1;
        }

        foreach (var version in seriesInfo.Versions
                     .Where(x => !x.Value.Metadata.Version.IsPrerelease || includePrerelease))
        {
            cancellationToken.ThrowIfCancellationRequested();
            AnsiConsole.WriteLine(version.Key);
        }

        return 0;
    }
    
    #if LEGACY_INTERFACE
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        return await ExecuteInternalAsync(settings.PackageName, settings.IncludePrerelease, _frontend.GetCancellationToken());
    }

    public sealed class Settings : BaseSettings
    {
        [CommandArgument(0, "<PACKAGE>")]
        [Description("The name of the package to list")]
        public required string PackageName { get; init; }

        [CommandOption("-P|--include-prerelease")]
        [Description("Whether to include prerelease when getting the latest version. Does not affect '--version'.")]
        public bool IncludePrerelease { get; init; }
    }
#endif
}

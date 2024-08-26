namespace IceCraft.Frontend.Commands;

using System.ComponentModel;
using System.Threading.Tasks;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Checksums;
using IceCraft.Core.Archive.Dependency;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Installation;
using IceCraft.Core.Installation.Analysis;
using IceCraft.Core.Network;
using IceCraft.Interactive;
using Spectre.Console;
using Spectre.Console.Cli;

public class PackageFixBrokenCommand : AsyncCommand<PackageFixBrokenCommand.Settings>
{
    private readonly IDependencyMapper _dependencyMapper;
    private readonly IPackageIndexer _indexer;
    private readonly IDependencyResolver _resolver;
    private readonly IRepositorySourceManager _sourceManager;

    private readonly InteractiveInstaller _installer;

    public PackageFixBrokenCommand(IDependencyMapper dependencyMapper,
        IPackageIndexer indexer,
        IDependencyResolver resolver,
        IRepositorySourceManager sourceManager,
        IDownloadManager downloadManager,
        IPackageInstallManager installManager,
        IArtefactManager artefactManager,
        IChecksumRunner checksumRunner)
    {
        _dependencyMapper = dependencyMapper;
        _indexer = indexer;
        _resolver = resolver;
        _sourceManager = sourceManager;

        _installer = new InteractiveInstaller(downloadManager,
            installManager,
            artefactManager,
            checksumRunner,
            dependencyMapper);
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var packages = new HashSet<PackageMeta>();
        var index = await _indexer.IndexAsync(_sourceManager);

        await foreach (var package in _dependencyMapper.EnumerateUnsatisifiedPackages())
        {
            await _resolver.ResolveTree(package, index, packages);
        }

        if (packages.Count == 0)
        {
            Output.Shared.Log("Dependencies are OK, nothing to install");
            return 0;
        }

        if (settings.DryRun)
        {
            AnsiConsole.Write(new Columns(packages.Select(p => p.Id)));
            return 0;
        }

        if (!_installer.AskConfirmation(packages))
        {
            return 0;
        }

        return await _installer.InstallAsync(packages, index);
    }

    public class Settings : BaseSettings
    {
        [CommandOption("--dry-run")]
        [Description("Do not install packages but list those will be installed.")]
        public bool DryRun { get; init; }
    }
}

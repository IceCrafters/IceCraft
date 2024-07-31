namespace IceCraft.Frontend;

using System.ComponentModel;
using IceCraft.Core;
using Spectre.Console.Cli;

[Description("Regenerate all package refs")]
public class UpdateCommand : AsyncCommand<BaseSettings>
{
    private readonly IRepositorySourceManager _sourceManager;

    public UpdateCommand(IRepositorySourceManager sourceManager)
    {
        _sourceManager = sourceManager;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, BaseSettings settings)
    {
        foreach (var source in _sourceManager.EnumerateSources())
        {
            await source.RefreshAsync();
        }

        return 0;
    }
}

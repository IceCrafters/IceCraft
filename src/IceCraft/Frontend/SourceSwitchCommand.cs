namespace IceCraft.Frontend;

using IceCraft.Core;
using IceCraft.Core.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;

public abstract class SourceSwitchCommand : Command<SourceSwitchCommand.Settings>
{
    private readonly IRepositorySourceManager _sourceManager;
    private readonly IManagerConfiguration _config;
    private readonly bool _toggle;

    protected SourceSwitchCommand(IRepositorySourceManager sourceManager,
        IManagerConfiguration config,
        bool state)
    {
        _sourceManager = sourceManager;
        _config = config;
        _toggle = state;
    }

    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (!_sourceManager.ContainsSource(settings.SourceName))
        {
            return ValidationResult.Error($"No such source: '{settings.SourceName}'");
        }

        return base.Validate(context, settings);
    }

    public class Settings : BaseSettings
    {
        [CommandArgument(0, "<SOURCE>")]
        public required string SourceName { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        _config.SetSourceEnabled(settings.SourceName, _toggle);
        return 0;
    }

    public sealed class EnableCommand : SourceSwitchCommand
    {
        public EnableCommand(IRepositorySourceManager sourceManager,
            IManagerConfiguration config)
             : base(sourceManager, config, true)
        {
        }
    }

    public sealed class DisableCommand : SourceSwitchCommand
    {
        public DisableCommand(IRepositorySourceManager sourceManager,
            IManagerConfiguration config)
             : base(sourceManager, config, false)
        {
        }
    }
}
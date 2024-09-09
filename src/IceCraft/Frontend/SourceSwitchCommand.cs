namespace IceCraft.Frontend;

using System.CommandLine;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Configuration;
using JetBrains.Annotations;

#if LEGACY_INTERFACE
using Spectre.Console;
using Spectre.Console.Cli;
#endif

using CliCommand = System.CommandLine.Command;

#if LEGACY_INTERFACE
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers,
    Reason = "Used by Program; see Spectre.Console.Cli docs and Program.cs")]
#endif
public abstract class SourceSwitchCommand
#if LEGACY_INTERFACE
    : Command<SourceSwitchCommand.Settings>
#endif
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

    public CliCommand CreateCli(string name)
    {
        var argSource = new Argument<string>("source", "The source to act on");

        var command = new CliCommand(name)
        {
            argSource
        };

        command.SetHandler(ExecuteInternal, argSource);
        return command;
    }

#if LEGACY_INTERFACE
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
        ExecuteInternal(settings.SourceName);
        return 0;
    }
#endif

    private void ExecuteInternal(string source)
    {
        _config.SetSourceEnabled(source, _toggle);
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
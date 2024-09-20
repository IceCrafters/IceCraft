// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Frontend;

using System.CommandLine;
using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Client;

using CliCommand = System.CommandLine.Command;

public abstract class SourceSwitchCommand
{
    private readonly IRepositorySourceManager _sourceManager;
    private readonly IManagerConfiguration _config;
    private readonly bool _toggle;

    private SourceSwitchCommand(IRepositorySourceManager sourceManager,
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
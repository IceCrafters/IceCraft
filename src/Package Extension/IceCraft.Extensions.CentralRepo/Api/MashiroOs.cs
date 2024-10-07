// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Api;

using IceCraft.Api.Platform;
using IceCraft.Extensions.CentralRepo.Runtime.Security;
using IceCraft.Extensions.CentralRepo.Util;

public class MashiroOs : ContextApi
{
    private readonly IEnvironmentManager _environmentManager;

    public MashiroOs(ContextApiRoot parent,
        IEnvironmentManager environmentManager) : base(ExecutionContextType.Installation | ExecutionContextType.Configuration, parent)
    {
        _environmentManager = environmentManager;
    }

    public int System(string command)
    {
        EnsureContext();
        
        if (!(OperatingSystem.IsWindows() || OperatingSystem.IsLinux()))
        {
            throw new PlatformNotSupportedException("Os.system is not supported on this platform.");
        }
        
#pragma warning disable CA1416
        return CommandShell.Execute(command);
#pragma warning restore CA1416
    }

    public void SetProcessEnv(string key, string value)
    {
        EnsureContext();

        _environmentManager.SetVariable(key, value, EnvironmentTarget.CurrentProcess);
    }

    public void RemoveProcessEnv(string key)
    {
        EnsureContext();

        _environmentManager.RemoveVariable(key, EnvironmentTarget.CurrentProcess);
    }

    public void AddProcessPath(string path)
    {
        EnsureContext();

        _environmentManager.AddPath(path, EnvironmentTarget.CurrentProcess);
    }

    public void RemoveProcessPath(string path)
    {
        EnsureContext();

        _environmentManager.RemovePath(path, EnvironmentTarget.CurrentProcess);
    }
}
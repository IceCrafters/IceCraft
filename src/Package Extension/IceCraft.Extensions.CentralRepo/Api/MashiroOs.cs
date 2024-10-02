// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Api;

using IceCraft.Extensions.CentralRepo.Runtime.Security;
using IceCraft.Extensions.CentralRepo.Util;

public class MashiroOs : ContextApi
{
    public MashiroOs(ContextApiRoot parent) : base(ExecutionContextType.Installation | ExecutionContextType.Configuration, parent)
    {
    }

    public int System(string command)
    {
        EnsureContext();
        
        if (!(OperatingSystem.IsWindows() || OperatingSystem.IsLinux()))
        {
            throw new PlatformNotSupportedException("Os.system is not supported on this platform.");
        }
        
        Console.WriteLine(command);
#pragma warning disable CA1416
        return CommandShell.Execute(command);
#pragma warning restore CA1416
    }
}
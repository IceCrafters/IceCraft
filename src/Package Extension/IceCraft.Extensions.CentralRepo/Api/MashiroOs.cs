namespace IceCraft.Extensions.CentralRepo.Api;

using IceCraft.Extensions.CentralRepo.Runtime.Security;
using IceCraft.Extensions.CentralRepo.Util;

public class MashiroOs : ContextApi
{
    public MashiroOs(ContextApiRoot parent) : base(ExecutionContextType.Installation | ExecutionContextType.Configuration, parent)
    {
    }

    public void System(string command)
    {
        EnsureContext();
        
        if (!(OperatingSystem.IsWindows() || OperatingSystem.IsLinux()))
        {
            throw new PlatformNotSupportedException("Os.system is not supported on this platform.");
        }
        
#pragma warning disable CA1416
        CommandShell.Execute(command);
#pragma warning restore CA1416
    }
}
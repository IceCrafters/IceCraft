namespace IceCraft.Extensions.CentralRepo.Runtime.Security;

[Flags]
public enum ExecutionContextType
{
    None,
    Metadata = 1,
    Installation = 3,
    Configuration = 7
}
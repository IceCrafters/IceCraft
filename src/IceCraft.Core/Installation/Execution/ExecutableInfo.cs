namespace IceCraft.Core.Installation.Execution;

public sealed record ExecutableInfo
{
    public required IDictionary<string, ExecutableRegistrationEntry> Registrations { get; init; }
    public ExecutableRegistrationEntry? Current { get; set; }
}

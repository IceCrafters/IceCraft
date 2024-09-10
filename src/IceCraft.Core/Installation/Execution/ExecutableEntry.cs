namespace IceCraft.Core.Installation.Execution;

using IceCraft.Api.Platform;
using IceCraft.Core.Platform;

public sealed record ExecutableEntry
{
    public required string LinkName { get; init; }
    public required string LinkTarget { get; init; }
    public required string PackageRef { get; init; }
    public EnvironmentVariableDictionary? Variables { get; init; }
}

namespace IceCraft.Core.Installation.Execution;

using System.Diagnostics.CodeAnalysis;

public readonly record struct ExecutableRegistrationEntry 
{
    [SetsRequiredMembers]
    public ExecutableRegistrationEntry(string packageRef, 
        string linkTarget,
        EnvironmentVariableDictionary? envVars = null)
    {
        PackageRef = packageRef;
        LinkTarget = linkTarget;
        EnvironmentVariables = envVars;
    }

    public required string PackageRef { get; init; }
    public required string LinkTarget { get; init; }
    public EnvironmentVariableDictionary? EnvironmentVariables { get; init; }
}

namespace IceCraft.Core.Archive.Packaging;

using System.Diagnostics.CodeAnalysis;

public readonly record struct PackagePluginInfo
{
    public PackagePluginInfo()
    {
    }

    [SetsRequiredMembers]
    public PackagePluginInfo(string installerRef, string configuratorRef, string? preProcessorRef = null)
    {
        InstallerRef = installerRef;
        ConfiguratorRef = configuratorRef;
        PreProcessorRef = preProcessorRef;
    }

    public required string InstallerRef { get; init; }

    public required string ConfiguratorRef { get; init; }

    public string? PreProcessorRef { get; init; }
}

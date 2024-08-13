namespace IceCraft.Core.Archive.Packaging;
using System;
using System.Diagnostics.CodeAnalysis;

public readonly record struct PackagePluginInfo
{
    public PackagePluginInfo()
    {
    }

    [SetsRequiredMembers]
    public PackagePluginInfo(string installerRef, string configuratorRef)
    {
        InstallerRef = installerRef;
        ConfiguratorRef = configuratorRef;
    }

    public required string InstallerRef { get; init; }

    public required string ConfiguratorRef { get; init; }
}

namespace IceCraft.Core.Archive.Packaging;
using System;
using System.Diagnostics.CodeAnalysis;

public readonly record struct PackagePluginInfo
{
    public PackagePluginInfo()
    {
    }

    [SetsRequiredMembers]
    public PackagePluginInfo(string installerRef)
    {
        InstallerRef = installerRef;
    }

    public required string InstallerRef { get; init; }
}

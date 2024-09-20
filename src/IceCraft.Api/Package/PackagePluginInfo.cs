// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Package;

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

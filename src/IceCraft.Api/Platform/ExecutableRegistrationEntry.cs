// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Platform;

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

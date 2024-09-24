// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation;

using System.Diagnostics.CodeAnalysis;
using IceCraft.Api.Package;

public readonly record struct DueInstallTask
{
    [SetsRequiredMembers]
    public DueInstallTask(PackageMeta package, string artefactPath, bool isExplicit)
    {
        Package = package;
        ArtefactPath = artefactPath;
        IsExplicit = isExplicit;
    }
    
    public required PackageMeta Package { get; init; }
    public required string ArtefactPath { get; init; }
    public required bool IsExplicit { get; init; }
}
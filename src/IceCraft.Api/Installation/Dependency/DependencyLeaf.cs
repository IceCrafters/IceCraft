// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Installation.Dependency;

using System.Diagnostics.CodeAnalysis;
using IceCraft.Api.Package;

public readonly record struct DependencyLeaf
{
    [SetsRequiredMembers]
    public DependencyLeaf(PackageMeta package, bool isExplicit)
    {
        Package = package;
        IsExplicit = isExplicit;
    }
    
    public required PackageMeta Package { get; init; }
    public required bool IsExplicit { get; init; }
}
// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Impl;

using System.Diagnostics.CodeAnalysis;

public readonly record struct RemotePackageData
{
    [SetsRequiredMembers]
    public RemotePackageData(string? fileName)
    {
        FileName = fileName;
    }
    
    public required string? FileName { get; init; }
}
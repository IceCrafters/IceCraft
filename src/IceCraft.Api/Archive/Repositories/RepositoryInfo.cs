// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Archive.Repositories;

using System.Diagnostics.CodeAnalysis;

public readonly record struct RepositoryInfo
{
    [SetsRequiredMembers]
    public RepositoryInfo(string name, IRepository repository)
    {
        Name = name;
        Repository = repository;
    }

    public required string Name { get; init; }
    public required IRepository Repository { get; init; }
}

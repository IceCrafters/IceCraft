// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.DotNet;

using System;
using IceCraft.Api.Archive.Repositories;

public class DotNetRepositorySourceFactory : IRepositorySourceFactory
{
    public IRepositorySource CreateRepositorySource(IServiceProvider provider, out string name)
    {
        name = "dotnet";
        return new DotNetRepositorySource();
    }
}

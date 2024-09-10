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

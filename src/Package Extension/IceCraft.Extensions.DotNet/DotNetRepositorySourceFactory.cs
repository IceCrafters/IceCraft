namespace IceCraft.Extensions.DotNet;

using System;
using IceCraft.Core.Archive.Providers;

public class DotNetRepositorySourceFactory : IRepositorySourceFactory
{
    public IRepositorySource CreateRepositorySource(IServiceProvider provider, out string name)
    {
        name = "dotnet";
        return new DotNetRepositorySource();
    }
}

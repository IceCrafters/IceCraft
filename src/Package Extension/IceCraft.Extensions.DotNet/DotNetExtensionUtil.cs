namespace IceCraft.Extensions.DotNet;

using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Installation;
using IceCraft.Core.Installation;
using Microsoft.Extensions.DependencyInjection;

public static class DotNetExtensionUtil
{
    public static IServiceCollection AddDotNetExtension(this IServiceCollection services)
    {
        return services.AddKeyedSingleton<IRepositorySourceFactory, DotNetRepositorySourceFactory>(null)
            .AddKeyedSingleton<IPackageInstaller, DotNetSdkInstaller>("dotnet-sdk")
            .AddKeyedSingleton<IPackageConfigurator, DotNetSdkConfigurator>("dotnet-sdk");
    }
}
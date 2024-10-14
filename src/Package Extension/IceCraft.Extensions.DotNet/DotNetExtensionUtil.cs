// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.DotNet;

using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Installation;
using Microsoft.Extensions.DependencyInjection;

public static class DotNetExtensionUtil
{
    public static IServiceCollection AddDotNetExtension(this IServiceCollection services)
    {
        return services.AddTransient<IRepositorySourceFactory, DotNetRepositorySourceFactory>()
            .AddKeyedScoped<IPackageInstaller, DotNetSdkInstaller>("dotnet-sdk")
            .AddKeyedScoped<IPackageConfigurator, DotNetSdkConfigurator>("dotnet-sdk");
    }
}
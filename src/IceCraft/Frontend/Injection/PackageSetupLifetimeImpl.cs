// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Frontend.Injection;

using Autofac;
using Autofac.Core;
using IceCraft.Api.Exceptions;
using IceCraft.Api.Installation;

internal class PackageSetupLifetimeImpl : IPackageSetupLifetime
{
    private readonly IComponentContext _context;

    public PackageSetupLifetimeImpl(IComponentContext context)
    {
        _context = context;
    }

    public IPackageConfigurator GetConfigurator(string id)
    {
        return Resolve<IPackageConfigurator>(id);
    }

    public IPackageInstaller GetInstaller(string id)
    {
        return Resolve<IPackageInstaller>(id);
    }

    public IArtefactPreprocessor GetPreprocessor(string id)
    {
        return Resolve<IArtefactPreprocessor>(id);
    }

    private T Resolve<T>(string id) where T: class
    {
        T? result;

        try
        {
            if (!_context.TryResolveKeyed<T>(id, out result))
            {
                throw CreateExServiceNotFound(typeof(T).Name, id, null);
            }
        }
        catch (DependencyResolutionException ex)
        {
            throw CreateExServiceNotFound(typeof(T).Name, id, ex);
        }

        return result;
    }

    private static PackageMetadataException CreateExServiceNotFound(string service, string serviceId, Exception? inner)
    {
        return new PackageMetadataException($"Unable to resolve {service} named {serviceId}.", inner);
    }
}

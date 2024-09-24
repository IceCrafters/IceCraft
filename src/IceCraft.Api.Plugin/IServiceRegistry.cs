// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Plugin;

using JetBrains.Annotations;

public interface IServiceRegistry
{
    IServiceRegistry RegisterSingleton<TInterface, 
        [MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)] TImplementation>()
        where TInterface : class
        where TImplementation : class, TInterface;

    IServiceRegistry RegisterSingleton<[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)] T>()
        where T : class;
    
    IServiceRegistry RegisterKeyedSingleton<TInterface, 
        [MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)] TImplementation>(string? key)
        where TInterface : class
        where TImplementation : class, TInterface;
}
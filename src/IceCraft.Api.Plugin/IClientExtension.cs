// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Plugin;

/// <summary>
/// Defines a plugin that extends the client. Implementations should also be
/// <see cref="IPlugin"/> implementations.
/// </summary>
public interface IClientExtension
{
    void InitializeClient(IExtensibleClient client, IServiceProvider serviceProvider);
}

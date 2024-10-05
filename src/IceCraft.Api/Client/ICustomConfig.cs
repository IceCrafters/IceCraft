// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Client;

[Obsolete("Use IConfigManager instead.")]
public interface ICustomConfig
{
    [Obsolete("Use IConfigManager instead.")]
    IConfigScope GetScope(string name);
}
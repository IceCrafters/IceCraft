// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Frontend.Configuration;

internal class ClientConfigModel
{
    public bool DoesAllowUncertainHash { get; set; }
    public required HashSet<string> EnabledSources { get; init; }
}
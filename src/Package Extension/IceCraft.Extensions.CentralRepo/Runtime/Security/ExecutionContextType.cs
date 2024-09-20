// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Runtime.Security;

[Flags]
public enum ExecutionContextType
{
    None,
    Metadata = 1,
    Installation = 2,
    Configuration = 4
}
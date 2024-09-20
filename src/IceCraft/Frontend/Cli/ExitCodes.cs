// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Frontend.Cli;

public static class ExitCodes
{
    public const int Ok = 0;
    public const int GenericError = 1;
    public const int PackageNotFound = 2;
    public const int ForceOptionRequired = 3;
    public const int Cancelled = 4;
}

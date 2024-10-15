// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Runtime;

using IceCraft.Extensions.CentralRepo.Api;
using IceCraft.Extensions.CentralRepo.Runtime.Security;

public sealed class MashiroApiProvider : IMashiroApiProvider
{
    public MashiroApiProvider(ContextApiRoot context,
        IMashiroFsApi fs,
        IMashiroAssetsApi assets,
        IMashiroBinaryApi binary,
        IMashiroCompressedArchiveApi compressedArchive,
        IMashiroConsoleApi console,
        IMashiroPackagesApi packages,
        IMashiroOsApi os)
    {
        Context = context;

        Fs = fs;
        Assets = assets;
        Binary = binary;
        CompressedArchive = compressedArchive;
        MConsole = console;
        Packages = packages;
        Os = os;
    }

    internal ContextApiRoot Context { get; }

    public IMashiroFsApi Fs { get; }
    public IMashiroAssetsApi Assets { get; }
    public IMashiroBinaryApi Binary { get; }
    public IMashiroCompressedArchiveApi CompressedArchive { get; }
    public IMashiroConsoleApi MConsole { get; }
    public IMashiroPackagesApi Packages { get; }
    public IMashiroOsApi Os { get; }
}

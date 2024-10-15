// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Runtime;

using IceCraft.Extensions.CentralRepo.Api;

public interface IMashiroApiProvider
{
    IMashiroFsApi Fs { get; }
    IMashiroAssetsApi Assets { get; }
    IMashiroBinaryApi Binary { get; }
    IMashiroCompressedArchiveApi CompressedArchive { get; }
    IMashiroConsoleApi MConsole { get; }
    IMashiroPackagesApi Packages { get; }
    IMashiroOsApi Os { get; }
}
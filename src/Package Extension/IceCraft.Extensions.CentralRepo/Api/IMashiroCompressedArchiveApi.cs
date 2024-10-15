// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Api;

public interface IMashiroCompressedArchiveApi
{
    void Expand(string archive, string destination, bool overwrite = true);
    void Expand(MashiroAssetHandle assetHandle, string destination, bool overwrite = false, bool leaveOpen = false);
}
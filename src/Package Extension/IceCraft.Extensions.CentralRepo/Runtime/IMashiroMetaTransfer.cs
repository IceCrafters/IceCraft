// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Runtime;

using IceCraft.Api.Package;

public interface IMashiroMetaTransfer
{
    public PackageMeta? PackageMeta { get; set; }
    public Action? EnsureMetadataAction { get; set; }
    
    void EnsureMetadata();
}
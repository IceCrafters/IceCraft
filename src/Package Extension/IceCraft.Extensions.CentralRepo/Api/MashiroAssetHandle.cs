// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Api;

public readonly record struct MashiroAssetHandle : IDisposable
{
    private readonly FileStream _fileStream;

    internal MashiroAssetHandle(FileStream fileStream)
    {
        _fileStream = fileStream;
    }
    
    internal Stream GetStream()
    {
        return _fileStream;
    }
    
    public void Dispose()
    {
        ((IDisposable)_fileStream).Dispose();
    }
}

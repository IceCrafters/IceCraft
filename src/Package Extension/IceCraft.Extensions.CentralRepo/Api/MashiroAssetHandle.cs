// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Api;

public readonly record struct MashiroAssetHandle : IDisposable
{
    private readonly string _fileName;
    private readonly FileStream _fileStream;

    internal MashiroAssetHandle(string fileName, FileStream fileStream)
    {
        _fileName = fileName;
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

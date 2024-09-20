// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Api;

using IceCraft.Extensions.CentralRepo.Runtime.Security;
using SharpCompress.Common;
using SharpCompress.Readers;

public class MashiroCompressedArchive : ContextApi
{
    public MashiroCompressedArchive(ContextApiRoot parent) : base(ExecutionContextType.Installation, parent)
    {
    }

    public void Expand(string archive, string destination, bool overwrite = true)
    {
        EnsureContext();
        using var stream = File.OpenRead(archive);
        using var reader = ReaderFactory.Open(stream);
        
        reader.WriteAllToDirectory(destination, new ExtractionOptions
        {
            ExtractFullPath = true,
            Overwrite = overwrite
        });
    }
}
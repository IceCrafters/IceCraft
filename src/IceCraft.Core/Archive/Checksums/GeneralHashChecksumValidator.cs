// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Archive.Checksums;

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IceCraft.Api.Archive.Artefacts;

public abstract class GeneralHashChecksumValidator : IChecksumValidator
{
    public bool CompareChecksum(string a, string b)
    {
        return a.Equals(b, StringComparison.OrdinalIgnoreCase);
    }

    public abstract Task<byte[]> GetChecksumBinaryAsync(Stream stream, CancellationToken cancellation = default);

    public string GetChecksumString(byte[] checksumBin)
    {
        return Convert.ToHexString(checksumBin);
    }
}

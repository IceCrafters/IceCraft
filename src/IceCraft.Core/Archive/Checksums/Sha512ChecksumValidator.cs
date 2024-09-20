// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Archive.Checksums;

using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

public class Sha512ChecksumValidator : GeneralHashChecksumValidator
{
    private readonly SHA512 _sha = SHA512.Create();

    public override Task<byte[]> GetChecksumBinaryAsync(Stream stream, CancellationToken cancellation = default)
    {
        return _sha.ComputeHashAsync(stream, cancellation);
    }
}

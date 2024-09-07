
namespace IceCraft.Core.Archive.Checksums;

using System.IO;
using System.Security.Cryptography;

public sealed class Sha256ChecksumValidator : GeneralHashChecksumValidator
{
    private readonly SHA256 _sha256 = SHA256.Create();
    public static readonly Sha256ChecksumValidator Shared = new();

    public override Task<byte[]> GetChecksumBinaryAsync(Stream stream, CancellationToken cancellation = default)
    {
        return _sha256.ComputeHashAsync(stream, cancellation);
    }
}

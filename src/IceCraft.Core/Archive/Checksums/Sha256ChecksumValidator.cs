
namespace IceCraft.Core.Archive.Checksums;

public sealed class Sha256ChecksumValidator : GeneralHashChecksumValidator
{
    public static readonly Sha256ChecksumValidator Shared = new();
}

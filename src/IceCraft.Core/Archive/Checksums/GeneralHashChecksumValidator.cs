namespace IceCraft.Core.Archive.Checksums;

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IceCraft.Api.Archive.Artefacts;
using IceCraft.Core.Archive.Artefacts;

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

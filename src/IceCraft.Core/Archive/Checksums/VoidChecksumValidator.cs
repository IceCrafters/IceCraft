using IceCraft.Api.Archive.Artefacts;

namespace IceCraft.Core.Archive.Checksums;

public class VoidChecksumValidator : IChecksumValidator
{
    public bool CompareChecksum(string a, string b)
    {
        return true;
    }

    public Task<byte[]> GetChecksumBinaryAsync(Stream stream, CancellationToken cancellation = default)
    {
        return Task.FromResult(Array.Empty<byte>());
    }

    public string GetChecksumString(byte[] checksumBin)
    {
        return "void";
    }
}

namespace IceCraft.Core.Archive.Checksums;

public abstract class GeneralHashChecksumValidator : IChecksumValidator
{
    public bool CompareChecksum(string a, string b)
    {
        return a.Equals(b, StringComparison.OrdinalIgnoreCase);
    }

    public string GetChecksumString(byte[] checksumBin)
    {
        return Convert.ToHexString(checksumBin);
    }
}

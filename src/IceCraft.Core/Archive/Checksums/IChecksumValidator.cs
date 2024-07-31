namespace IceCraft.Core.Archive.Checksums;

public interface IChecksumValidator
{
    /// <summary>
    /// Gets the string representation of the specified checksum.
    /// </summary>
    /// <param name="checksumBin">The checksum in binary format.</param>
    /// <returns>The checksum.</returns>
    string GetChecksumString(byte[] checksumBin);

    /// <summary>
    /// Compares two checksums in string representation.
    /// </summary>
    /// <param name="a">The checksum A.</param>
    /// <param name="b">The checksum B.</param>
    /// <returns>Whether the checksum matches.</returns>
    bool CompareChecksum(string a, string b);

    Task<byte[]> GetChecksumBinaryAsync(Stream stream, CancellationToken cancellation = default);
}

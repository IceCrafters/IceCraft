namespace IceCraft.Api.Exceptions;

/// <summary>
/// Defines an exception that is thrown when there are error(s) present in the metadata
/// of a package.
/// </summary>
public class PackageMetadataException : Exception
{
    public PackageMetadataException()
    {
    }

    public PackageMetadataException(string? message) : base(message)
    {
    }

    public PackageMetadataException(string? message, string? packageId) : base(message)
    {
        PackageId = packageId;
    }

    public PackageMetadataException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public string? PackageId { get; }
}

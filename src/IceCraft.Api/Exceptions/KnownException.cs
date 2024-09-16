namespace IceCraft.Api.Exceptions;

/// <summary>
/// Represents an exception that is expected to throw and the full details should only be
/// shown under verbose mode.
/// </summary>
public class KnownException : Exception
{
    protected KnownException()
    {
    }

    public KnownException(string? message) : base(message)
    {
    }

    public KnownException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
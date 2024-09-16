namespace IceCraft.Api.Exceptions;

public class KnownInvalidOperationException : KnownException
{
    public KnownInvalidOperationException()
    {
    }

    public KnownInvalidOperationException(string? message) : base(message)
    {
    }

    public KnownInvalidOperationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

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
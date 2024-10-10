// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Api;

using IceCraft.Extensions.CentralRepo.Runtime;

public class MashiroFormatError : FormatException, IMashiroApiError
{
    public MashiroFormatError()
    {
    }

    public MashiroFormatError(string? message) : base(message)
    {
    }

    public MashiroFormatError(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    internal static MashiroFormatError CreateNotEnoughArguments(int position, int length)
    {
        return new MashiroFormatError($"Not enough arguments: requires argument #{position} but only have #{length + 1} arguments");
    }

    internal static MashiroFormatError NotNumber(int position)
    {
        return new MashiroFormatError($"Requires argument #{position} to be a number but it is not");
    }

    internal static MashiroFormatError UnknownSpecifier(char ch)
    {
        return new MashiroFormatError($"Unknown format specifier: {ch}");
    }
}

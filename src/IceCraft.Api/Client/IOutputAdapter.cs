// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Client;

using JetBrains.Annotations;

public interface IOutputAdapter
{
    void Error(string message);
    [StringFormatMethod("format")]
    void Error(string format, params object?[] args);

    void Warning(string message);
    void Warning(Exception exception, string message);
    [StringFormatMethod("format")]
    void Warning(string format, params object?[] args);

    [StringFormatMethod("format")]
    void Warning(Exception exception, string format, params object?[] args)
    {
        Warning(exception, string.Format(format, args));
    }

    void Log(string message);
    [StringFormatMethod("format")]
    void Log(string format, params object?[] args);

    void Verbose(string message);
    [StringFormatMethod("format")]
    void Verbose(string format, params object?[] args);

    void Tagged(string tag, string message);
    [StringFormatMethod("format")]
    void Tagged(string tag, string format, params object?[] args);
}

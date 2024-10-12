// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft;
using System;
using System.Runtime.CompilerServices;

internal static class Util
{
    /// <summary>
    /// Assets that the file name is valid.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="argName">The name of the argument of the file name.</param>
    /// <exception cref="ArgumentException">The file name is invalid.</exception>
    /// <exception cref="ArgumentNullException">The file name is null.</exception>
    internal static void CheckFileName(string? fileName, [CallerArgumentExpression(nameof(fileName))] string? argName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var invalidChars = Path.GetInvalidFileNameChars();
        foreach (var x in invalidChars)
        {
            if (fileName.Contains(x))
            {
                throw new ArgumentException($"File name '{fileName}' is invalid.", argName);
            }
        }
    }
}

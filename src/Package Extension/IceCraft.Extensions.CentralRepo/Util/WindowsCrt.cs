// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Util;

using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[SupportedOSPlatform("windows")]
internal static partial class WindowsCrt
{
    /// <summary>
    /// Gets the current value of the <c>errno</c> global variable.
    /// </summary>
    /// <param name="pValue">A pointer to an integer to be filled with the current value of the <c>errno</c> variable.</param>
    /// <returns>Returns zero if successful; an error code on failure. If pValue is NULL, the invalid parameter handler is invoked as described in Parameter validation. If execution is allowed to continue, this function sets errno to EINVAL and returns EINVAL.</returns>
    [LibraryImport("msvcrt.dll", StringMarshalling = StringMarshalling.Utf16)]
    internal static unsafe partial int _get_errno(int* pValue);

    [LibraryImport("msvcrt.dll", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial int _wsystem(string command);
}
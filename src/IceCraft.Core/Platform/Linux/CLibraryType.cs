// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Core.Platform.Linux;

public enum CLibraryType
{
    /// <summary>
    /// Unknown C Library.
    /// </summary>
    Unknown,
    /// <summary>
    /// The GNU C Library (<c>glibc</c>).
    /// </summary>
    Gnu,
    /// <summary>
    /// The <c>musl</c> C Library.
    /// </summary>
    Musl
}

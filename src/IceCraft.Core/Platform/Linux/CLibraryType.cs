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

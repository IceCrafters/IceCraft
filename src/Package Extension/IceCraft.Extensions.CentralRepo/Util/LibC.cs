namespace IceCraft.Extensions.CentralRepo.Util;

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[SupportedOSPlatform("linux")]
public static partial class LibC
{
    [LibraryImport("libc.so.6", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    public static partial int system(string command);

    private const int EAgain = 11;
    private const int ENoMem = 12;
    private const int ENoSys = 38;
    
    internal static Exception CreateForFork(int errno)
    {
        return errno switch
        {
            EAgain => throw new InvalidOperationException("Cannot allocate thread for process creation."),
            ENoMem => throw new OutOfMemoryException("Cannot allocate memory for process creation."),
            ENoSys => throw new NotSupportedException("Process creation is not supported on this device."),
            _ => throw new Win32Exception(Marshal.GetLastPInvokeError(), Marshal.GetLastPInvokeErrorMessage())
        };
    }
}
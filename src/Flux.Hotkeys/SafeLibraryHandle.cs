using System;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace Flux.Hotkeys;

internal sealed class SafeLibraryHandle() : SafeHandleZeroOrMinusOneIsInvalid(true)
{
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern SafeLibraryHandle LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FreeLibrary(IntPtr hModule);

    protected override bool ReleaseHandle() 
    { 
        return FreeLibrary(handle); 
    }
}
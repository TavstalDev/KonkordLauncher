using System;
using System.Runtime;
using System.Runtime.InteropServices;
using Tavstal.BeaconSDK.Enums;
using Tavstal.BeaconSDK.Helpers.Platform;

namespace Tavstal.KonkordLauncher.Desktop.Helpers;

/// <summary>
///     Helpers that free unused memory on different operating systems.
/// </summary>
public static class MemoryHelper
{
    /// <summary>
    ///     Removes as many pages as possible from the working set of a process.
    /// </summary>
    /// <param name="hProcess">A handle to the target process.</param>
    /// <returns><see langword="true" /> on success; otherwise, <see langword="false" />.</returns>
    [DllImport("psapi.dll", SetLastError = true)]
    public static extern bool EmptyWorkingSet(IntPtr hProcess);

    /// <summary>
    ///     Releases free memory from the heap back to the operating system.
    /// </summary>
    /// <param name="pad">The free space to keep at the top of the heap, in bytes.</param>
    /// <returns><c>1</c> if memory was released; otherwise, <c>0</c>.</returns>
    [DllImport("libc", SetLastError = true)]
    public static extern int malloc_trim(nuint pad);

    /// <summary>
    ///     Frees unused memory and returns it to the operating system.
    /// </summary>
    public static void Clean()
    {
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(2, GCCollectionMode.Aggressive, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();

        switch (OSHelper.GetOperatingSystem())
        {
            case EOperatingSystem.WINDOWS:
            {
                EmptyWorkingSet(-1);
                break;
            }
            case EOperatingSystem.LINUX:
            {
                try
                {
                    malloc_trim(0);
                }
                catch
                {
                    Console.WriteLine("malloc failed");
                    // Fails silently on non-glibc distros (like Alpine Linux using musl)
                }
                break;
            }
            case EOperatingSystem.MACOS:
            {

                break;
            }
        }
    }
}

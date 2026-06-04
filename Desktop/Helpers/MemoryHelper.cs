using System;
using System.Runtime.InteropServices;

namespace Tavstal.KonkordLauncher.Desktop.Helpers;

/// <summary>
/// Provides native memory management utility functions for optimizing application memory usage across different operating systems.
/// </summary>
public static class MemoryHelper
{
    /// <summary>
    /// Minimizes the working set of the specified process by removing as many pages as possible from its working set.
    /// </summary>
    /// <param name="hProcess">A handle to the process whose working set is to be emptied.</param>
    /// <returns>
    /// <see langword="true"/> if the function succeeds; otherwise, <see langword="false"/>. 
    /// To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.
    /// </returns>
    [DllImport("psapi.dll", SetLastError = true)]
    public static extern bool EmptyWorkingSet(IntPtr hProcess);
    
    /// <summary>
    /// Releases free memory from the heap back to the operating system.
    /// </summary>
    /// <param name="pad">The amount of free space to leave untrimmed at the top of the heap, in bytes.</param>
    /// <returns>
    /// Returns <c>1</c> if memory was actually released to the operating system, 
    /// or <c>0</c> if it was not possible to release any memory.
    /// </returns>
    [DllImport("libc", SetLastError = true)]
    public static extern int malloc_trim(nuint pad);
}
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AulaTray;

/// <summary>
/// Sistem tepsisi uygulamasının RAM kullanımını (Working Set)
/// 75 MB seviyelerinden 10-15 MB seviyelerine çeken hafif bellek optimizasyon servisi.
/// </summary>
public static class MemoryOptimizer
{
    [DllImport("psapi.dll")]
    private static extern int EmptyWorkingSet(IntPtr hwProc);

    /// <summary>
    /// Kullanılmayan JIT ve geçici sayfa tahsislerini işletim sistemine geri iade eder.
    /// </summary>
    public static void TrimWorkingSet()
    {
        try
        {
            GC.Collect(0, GCCollectionMode.Optimized, false);
            using var proc = Process.GetCurrentProcess();
            EmptyWorkingSet(proc.Handle);
        }
        catch { }
    }
}

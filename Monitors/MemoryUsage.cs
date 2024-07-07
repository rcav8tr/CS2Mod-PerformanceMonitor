using System;
using System.Runtime.InteropServices;

namespace PerformanceMonitor
{
    /// <summary>
    /// Memory usage for Windows only.
    /// </summary>
    public class MemoryUsage
    {
        // Memory status structure.
        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORYSTATUSEX {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        };
        private static MEMORYSTATUSEX _mem = new MEMORYSTATUSEX() { dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX)) };

        // External Windows call.
        [DllImport("kernel32")]
        private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX memoryStatus);
        
        /// <summary>
        /// Get memory usage as a percent.
        /// </summary>
        public static uint GetMemoryUsage()
        {
            try
            {
                // Return memory load.
                if (GlobalMemoryStatusEx(ref _mem))
                {
                    return _mem.dwMemoryLoad;
                }

                // Fail, return 0.
                return 0;
            }
            catch(Exception ex)
            {
                LogUtil.Exception(ex);
                return 0;
            }
        }
    }
}

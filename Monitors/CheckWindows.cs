using System.Runtime.InteropServices;

namespace PerformanceMonitor
{
    public static class CheckWindows
    {
        /// <summary>
        /// Return whether or not operating system is Windows.
        /// </summary>
        public static bool IsWindows()
        {
            try
            {
                // Get Windows status.
                return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            }
            catch
            {
                // For any exception, simply assume not Windows with no message.
                return false;
            }
        }
    }
}

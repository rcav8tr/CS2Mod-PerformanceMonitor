using System;
using System.Runtime.InteropServices;

namespace PerformanceMonitor
{
    /// <summary>
    /// CPU usage for Windows only.
    /// </summary>
    public static class CPUUsage
    {
        // Define file time.
        struct FileTime {
            // These two values together define the number of 100-nanosecond intervals.
            uint lowDateTime;
            uint highDateTime;

            // Return the two values combined into one ulong.
            public ulong ToULong()
            {
                return ((ulong)highDateTime << 32) + (ulong)lowDateTime;
            }
        };

        // External Windows call.
        [DllImport("kernel32")]
        private static extern bool GetSystemTimes(out FileTime idleTime, out FileTime kernelTime, out FileTime userTime);

        // Previous idle and total times.
        private static ulong _previousIdleTime;
        private static ulong _previousTotalTime;

        // Previous CPU usages for averaging.
        private static int _previousCPUUsageCount;
        private static double _previousCPUUsage1;
        private static double _previousCPUUsage2;

        /// <summary>
        /// Initialize before making any request for CPU usage.
        /// </summary>
        public static bool Initialize()
        {
            try
            {
                LogUtil.Info($"{nameof(CPUUsage)}.{nameof(Initialize)}");

                // Initialize previous CPU usages.
                _previousCPUUsageCount = 0;
                _previousCPUUsage1 = 0d;
                _previousCPUUsage2 = 0d;

                // Initialize previous times.
                return GetCurrentTimes(out _previousIdleTime, out _previousTotalTime);
            }
            catch(Exception ex)
            {
                LogUtil.Exception(ex);
                return false;
            }
        }

        /// <summary>
        /// Get CPU usage as a percent.
        /// </summary>
        public static int GetCPUUsage()
        {
            try
            {
                // Get current idle and total times.
                GetCurrentTimes(out ulong currentIdleTime, out ulong currentTotalTime);

                // Compute delta idle and total times.
                ulong deltaIdleTime  = currentIdleTime  - _previousIdleTime;
                ulong deltaTotalTime = currentTotalTime - _previousTotalTime;

                // Save idle and total times.
                _previousIdleTime  = currentIdleTime;
                _previousTotalTime = currentTotalTime;

                // Idle percent is delta idle time divided by delta total time.
                // Total time includes idle time, so the delta total time "should" never be zero.
                // But prevent divide by zero, just in case.
                double idlePercent = 0d;
                if (deltaTotalTime != 0UL)
                {
                    idlePercent = 100d * deltaIdleTime / deltaTotalTime;
                }

                // Current CPU usage is 100% minus idle percent (i.e. any time not idle is considered used).
                // Cannot be less than 0% or more than 100%.
                double currentCPUUsage = 100d - idlePercent;
                if (currentCPUUsage < 0d)
                {
                    currentCPUUsage = 0d;
                }
                if (currentCPUUsage > 100d)
                {
                    currentCPUUsage = 100d;
                }

                // Compute weighted average CPU usage.
                double averageCPUUsage;
                _previousCPUUsageCount++;
                if      (_previousCPUUsageCount >= 3) { averageCPUUsage = (currentCPUUsage * 4d + _previousCPUUsage1 * 2d + _previousCPUUsage2 + 1d) / 7d; }
                else if (_previousCPUUsageCount == 2) { averageCPUUsage = (currentCPUUsage * 2d + _previousCPUUsage1 * 1d                          ) / 3d; }
                else                                  { averageCPUUsage =  currentCPUUsage;                                                                }
                _previousCPUUsage2 = _previousCPUUsage1;
                _previousCPUUsage1 = currentCPUUsage;

                // Return weighted average CPU usage rounded to an int.
                return (int)Math.Round(averageCPUUsage, 0);
            }
            catch(Exception ex)
            {
                LogUtil.Exception(ex);
                return 0;
            }
        }

        /// <summary>
        /// Get current idle and total times.
        /// </summary>
        private static bool GetCurrentTimes(out ulong currentIdleTime, out ulong currentTotalTime)
        {
            // Get system times.
            // Kernel time includes idle time.
            if (GetSystemTimes(out FileTime idleTime, out FileTime kernelTime, out FileTime userTime))
            {
                // Return current idle and total times.
                currentIdleTime = idleTime.ToULong();
                currentTotalTime = kernelTime.ToULong() + userTime.ToULong();

                // Success.
                return true;
            }

            // Failure.
            currentIdleTime  = 0;
            currentTotalTime = 0;
            return false;
        }
    }
}

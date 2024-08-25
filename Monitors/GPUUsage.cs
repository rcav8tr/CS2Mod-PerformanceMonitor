using System;
using System.Runtime.InteropServices;

namespace PerformanceMonitor
{
    /// <summary>
    /// GPU usage for Windows only and for NVidia only.
    /// </summary>
    public static class GPUUsage
    {
        // NVML is NVIDIA Management Library.
        // NVML API definitions adapted from:  https://github.com/jcbritobr/nvml-csharp
        // See also:  https://docs.nvidia.com/deploy/nvml-api/nvml-api-reference.html

        // Define NVML return values.
        private enum NvmlReturn
        {
            NVML_SUCCESS = 0,
            NVML_ERROR_UNINITIALIZED,
            NVML_ERROR_INVALID_ARGUMENT,
            NVML_ERROR_NOT_SUPPORTED,
            NVML_ERROR_NO_PERMISSION,
            NVML_ERROR_ALREADY_INITIALIZED,
            NVML_ERROR_NOT_FOUND,
            NVML_ERROR_INSUFFICIENT_SIZE,
            NVML_ERROR_INSUFFICIENT_POWER,
            NVML_ERROR_DRIVER_NOT_LOADED,
            NVML_ERROR_TIMEOUT,
            NVML_ERROR_IRQ_ISSUE,
            NVML_ERROR_LIBRARY_NOT_FOUND,
            NVML_ERROR_FUNCTION_NOT_FOUND,
            NVML_ERROR_CORRUPTED_INFOROM,
            NVML_ERROR_GPU_IS_LOST,
            NVML_ERROR_RESET_REQUIRED,
            NVML_ERROR_OPERATING_SYSTEM,
            NVML_ERROR_LIB_RM_VERSION_MISMATCH,
            NVML_ERROR_IN_USE,
            NVML_ERROR_MEMORY,
            NVML_ERROR_NO_DATA,
            NVML_ERROR_VGPU_ECC_NOT_SUPPORTED,
            NVML_ERROR_INSUFFICIENT_RESOURCES,
            NVML_ERROR_FREQ_NOT_SUPPORTED,
            NVML_ERROR_ARGUMENT_VERSION_MISMATCH,
            NVML_ERROR_DEPRECATED,
            NVML_ERROR_NOT_READY,
            NVML_ERROR_GPU_NOT_FOUND,
            NVML_ERROR_INVALID_STATE,
            NVML_ERROR_UNKNOWN = 999
        }

        // Define NVML utilization structure.
        private struct NvmlUtilization
        {
            public uint gpu;
            public uint memory;
        }

        // Define needed NVML API routines.
        private const string NVMLLibrary = "nvml.dll";

        [DllImport(NVMLLibrary, EntryPoint = "nvmlInit_v2")]
        private static extern NvmlReturn NvmlInitV2();

        [DllImport(NVMLLibrary, EntryPoint = "nvmlShutdown")]
        private static extern NvmlReturn NvmlShutdown();

        [DllImport(NVMLLibrary, CharSet = CharSet.Ansi, EntryPoint = "nvmlDeviceGetCount_v2")]
        private static extern NvmlReturn NvmlDeviceGetCount_v2(out uint deviceCount);

        [DllImport(NVMLLibrary, EntryPoint = "nvmlDeviceGetHandleByIndex")]
        private static extern NvmlReturn NvmlDeviceGetHandleByIndex(uint index, out IntPtr device);

        [DllImport(NVMLLibrary, EntryPoint = "nvmlDeviceGetUtilizationRates")]
        private static extern NvmlReturn NvmlDeviceGetUtilizationRates(IntPtr device, out NvmlUtilization utilization);

        // Device handles.
        private static IntPtr[] _devices;

        // Initialization flag.
        private static bool _initialized = false;

        /// <summary>
        /// Initialize the GPU usage and return validity status.
        /// Should never be called more than once.
        /// </summary>
        public static bool Initialize()
        {
            try
            {
                LogUtil.Info($"{nameof(GPUUsage)}.{nameof(Initialize)}");

                // Check if already initialized.
                if (_initialized)
                {
                    LogUtil.Warn("Attempt to initialize GPU usage more than onece.");
                    return true;
                }

                // Initialize NVML.
                NvmlReturn ret = NvmlInitV2();
                if (ret != NvmlReturn.NVML_SUCCESS)
                {
                    LogUtil.Warn($"NVML error initializing: [{ret}]");
                    return false;
                }

                // Get number of devices.
                ret = NvmlDeviceGetCount_v2(out uint deviceCount);
                if (ret != NvmlReturn.NVML_SUCCESS)
                {
                    LogUtil.Warn($"NVML error getting device count: [{ret}]");
                    return false;
                }

                // Check number of devices.
                if (deviceCount == 0)
                {
                    LogUtil.Warn($"NVML error no devices found.");
                    return false;
                }

                // Get device handle for each device.
                _devices = new IntPtr[deviceCount];
                for (uint i = 0; i < deviceCount; i++)
                {
                    ret = NvmlDeviceGetHandleByIndex(i, out _devices[i]);
                    if (ret != NvmlReturn.NVML_SUCCESS)
                    {
                        LogUtil.Warn($"NVML error getting handle for device #{i}: [{ret}]");
                        return false;
                    }
                }

                // Check that utilization can be obtained from every device.
                for (int i = 0; i < deviceCount; i++)
                {
                    ret = NvmlDeviceGetUtilizationRates(_devices[i], out NvmlUtilization utilization);
                    if (ret != NvmlReturn.NVML_SUCCESS)
                    {
                        LogUtil.Warn($"NVML error getting utilization from device #{i}: [{ret}]");
                        return false;
                    }
                }
                
                // GPU usage is valid.
                _initialized = true;
                LogUtil.Info("GPUUsage successfully initialized.");
                return true;
            }
            catch(DllNotFoundException)
            {
                LogUtil.Warn("NVML DLL not found during initialization, probably because GPU is not NVidia.");
                return false;
            }
            catch(Exception ex)
            {
                LogUtil.Exception(ex);
                return false;
            }
        }

        /// <summary>
        /// Shtudown GPU usage.
        /// </summary>
        public static void Shutdown()
        {
            try
            {
                // Shutdown only if initialized.
                if (_initialized)
                {
                    // Shut down NVML.  Ignore any error in the return value.
                    NvmlReturn ret = NvmlShutdown();
                    _initialized = false;
                }
            }
            catch(DllNotFoundException)
            {
                LogUtil.Warn("NVML DLL not found during shutdown, probably because GPU is not NVidia.");
            }
            catch(Exception ex)
            {
                LogUtil.Exception(ex);
            }
        }

        /// <summary>
        /// Get GPU usage as a percent.
        /// </summary>
        public static int GetGPUUsage()
        {
            try
            {
                // Get total utilization from all devices.
                uint total = 0U;
                for (int i = 0; i < _devices.Length; i++)
                {
                    NvmlReturn ret = NvmlDeviceGetUtilizationRates(_devices[i], out NvmlUtilization utilization);
                    if (ret == NvmlReturn.NVML_SUCCESS)
                    {
                        total += utilization.gpu;
                    }
                }

                // Compute average GPU usage over the devices, not more than 100 percent.
                double gpuUsage = (double)total / _devices.Length;
                if (gpuUsage > 100d)
                {
                    gpuUsage = 100d;
                }

                // Return the GPU usage as an integer.
                return (int)gpuUsage;
            }
            catch(Exception ex)
            {
                LogUtil.Exception(ex);
                return 0;
            }
        }
    }
}

using Core.Interfaces;
using Core.Models;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

using Microsoft.Extensions.Logging;

namespace Impl.Monitors
{
    /// <summary>
    /// Windows-specific system monitoring implementation using PerformanceCounters
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class WindowsSystemMonitor : ISystemMonitor, IDisposable
    {
        private readonly ILogger<WindowsSystemMonitor> myLogger;
        private readonly  PerformanceCounter myCpuCounter;
        private bool myDisposed = false;

        public WindowsSystemMonitor(ILogger<WindowsSystemMonitor> logger)
        {
            myLogger = logger ?? throw new ArgumentNullException(nameof(logger));

            try
            {
                myCpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                // Initialize the counter by calling NextValue() once
                myCpuCounter.NextValue();
            }
            catch (Exception ex)
            {
                myLogger.LogError(ex, "Failed to initialize CPU performance counter.");
                myCpuCounter = null!;
            }
        }

        public async Task<SystemMetrics> GetSystemMetricsAsync()
        {
            try
            {
                var metrics = new SystemMetrics();

                // Get CPU usage
                metrics.CpuUsagePercent = await GetCpuUsageAsync();

                // Get memory usage
                var (ramUsed, ramTotal) = await GetMemoryUsageAsync();
                metrics.RamUsedMb = ramUsed;
                metrics.RamTotalMb = ramTotal;

                // Get disk usage (C: drive by default)
                var (diskUsed, diskTotal) = await GetDiskUsageAsync();
                metrics.DiskUsedMb = diskUsed;
                metrics.DiskTotalMb = diskTotal;

                return metrics;
            }
            catch (Exception ex)
            {
                myLogger.LogError(ex, "Error collecting system metrics");
                throw;
            }
        }

        private async Task<double> GetCpuUsageAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (myCpuCounter == null)
                    {
                        myLogger.LogWarning("CPU counter not available, returning 0");
                        return 0.0;
                    }

                    // Wait a bit for accurate reading
                    Thread.Sleep(100);
                    return Math.Round(myCpuCounter.NextValue(), 2);
                }
                catch (Exception ex)
                {
                    myLogger.LogError(ex, "Error getting CPU usage");
                    return 0.0;
                }
            });
        }

        private async Task<(long used, long total)> GetMemoryUsageAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Use PerformanceCounters for accurate system memory info
                    var availableMemoryCounter = new PerformanceCounter("Memory", "Available MBytes");
                    var availableMemoryMb = (long)availableMemoryCounter.NextValue();

                    // Estimate total memory based on available memory patterns
                    var estimatedTotalMemoryMb = Math.Max(8192, availableMemoryMb * 4);
                    var usedMemoryMb = estimatedTotalMemoryMb - availableMemoryMb;

                    return (usedMemoryMb, estimatedTotalMemoryMb);
                }
                catch (Exception ex)
                {
                    myLogger.LogError(ex, "Error getting memory usage, using GC info as fallback");
                    // Fallback to GC memory info
                    var totalMemory = GC.GetTotalMemory(false) / (1024 * 1024);
                    return (totalMemory, Math.Max(totalMemory * 2, 8192)); // Rough estimation with 8GB minimum
                }
            });
        }

        private async Task<(long used, long total)> GetDiskUsageAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var drives = DriveInfo.GetDrives()
                        .Where(d => d is { IsReady: true, DriveType: DriveType.Fixed })
                        .ToList();

                    if (!drives.Any())
                    {
                        myLogger.LogWarning("No ready fixed drives found");
                        return (0L, 0L);
                    }

                    // Use C: drive or first available drive
                    var primaryDrive = drives.FirstOrDefault(d => d.Name.StartsWith("C:")) ?? drives.First();

                    var totalSize = primaryDrive.TotalSize / (1024 * 1024);
                    var freeSpace = primaryDrive.AvailableFreeSpace / (1024 * 1024);
                    var usedSpace = totalSize - freeSpace;

                    return (usedSpace, totalSize);
                }
                catch (Exception ex)
                {
                    myLogger.LogError(ex, "Error getting disk usage");
                    return (0L, 0L);
                }
            });
        }

        public void Dispose()
        {
            if (!myDisposed)
            {
                myCpuCounter.Dispose();
                myDisposed = true;
            }
        }
    }
}

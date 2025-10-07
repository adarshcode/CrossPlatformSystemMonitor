using System.Runtime.InteropServices;

using Microsoft.Extensions.Logging;

using Core.Interfaces;
using Core.Models;


namespace Impl.Monitors
{
    /// <summary>
    /// Cross-platform system monitoring implementation
    /// </summary>
    public class SystemMonitor : ISystemMonitor
    {
        private readonly ILogger<SystemMonitor> myLogger;

        private readonly ISystemMonitor? myPlatformSpecificMonitor;

        public SystemMonitor(ILogger<SystemMonitor> logger)
        {
            myLogger = logger ?? throw new ArgumentNullException(nameof(logger));

            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            // Create platform-specific monitor
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var windowsLogger = loggerFactory.CreateLogger<WindowsSystemMonitor>();
                myPlatformSpecificMonitor = new WindowsSystemMonitor(windowsLogger);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
               //TODO
            }
        }

        public async Task<SystemMetrics> GetSystemMetricsAsync()
        {
            return await myPlatformSpecificMonitor.GetSystemMetricsAsync();
        }
    }
}

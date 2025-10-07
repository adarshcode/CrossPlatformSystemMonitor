using Core.Interfaces;
using Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins
{
    /// <summary>
    /// Plugin that logs metrics to a local file
    /// </summary>
    public class LoggingPlugin : IMonitorPlugin
    {
        private readonly ILogger<LoggingPlugin> myLogger;
        private readonly string myLogFilePath;
        private readonly SemaphoreSlim myFileSemaphore;

        public string Name => "File Logger Plugin";

        public LoggingPlugin(ILogger<LoggingPlugin> logger, string logFilePath = "system-monitor.log")
        {
            myLogger = logger ?? throw new ArgumentNullException(nameof(logger));
            myLogFilePath = logFilePath ?? throw new ArgumentNullException(nameof(logFilePath));
            myFileSemaphore = new SemaphoreSlim(1, 1);
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Ensure the directory exists
                var directory = Path.GetDirectoryName(myLogFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Write header to log file
                await myFileSemaphore.WaitAsync(cancellationToken);
                try
                {
                    await File.AppendAllTextAsync(myLogFilePath,
                        $"=== System Monitor Started at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC ==={Environment.NewLine}",
                        cancellationToken);
                }
                finally
                {
                    myFileSemaphore.Release();
                }

                myLogger.LogInformation("File logger plugin initialized. Logging to: {LogFilePath}", myLogFilePath);
            }
            catch (Exception ex)
            {
                myLogger.LogError(ex, "Failed to initialize file logger plugin");
                throw;
            }
        }

        public async Task ProcessMetricsAsync(SystemMetrics metrics, CancellationToken cancellationToken = default)
        {
            try
            {
                var logEntry = $"{metrics.Timestamp:yyyy-MM-dd HH:mm:ss} | {metrics}{Environment.NewLine}";

                await myFileSemaphore.WaitAsync(cancellationToken);
                try
                {
                    await File.AppendAllTextAsync(myLogFilePath, logEntry, cancellationToken);
                }
                finally
                {
                    myFileSemaphore.Release();
                }
            }
            catch (Exception ex)
            {
                myLogger.LogError(ex, "Failed to log metrics to file");
            }
        }

        public async Task CleanupAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await myFileSemaphore.WaitAsync(cancellationToken);
                try
                {
                    await File.AppendAllTextAsync(myLogFilePath,
                        $"=== System Monitor Stopped at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC ==={Environment.NewLine}",
                        cancellationToken);
                }
                finally
                {
                    myFileSemaphore.Release();
                }

                myFileSemaphore?.Dispose();
                myLogger.LogInformation("File logger plugin cleaned up");
            }
            catch (Exception ex)
            {
                myLogger.LogError(ex, "Failed to cleanup file logger plugin");
            }
        }
    }
}

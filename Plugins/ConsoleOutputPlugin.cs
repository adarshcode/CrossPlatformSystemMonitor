using Core.Interfaces;
using Core.Models;
using Microsoft.Extensions.Logging;

namespace Plugins
{
    /// <summary>
    /// Plugin that outputs metrics to the console
    /// </summary>
    public class ConsoleOutputPlugin : IMonitorPlugin
    {
        private readonly ILogger<ConsoleOutputPlugin> myLogger;

        public string Name => "Console Output Plugin";

        public ConsoleOutputPlugin(ILogger<ConsoleOutputPlugin> logger)
        {
            myLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine();
            Console.WriteLine("=== System Monitor Console Output Started ===");
            Console.WriteLine("Monitoring system resources...");
            Console.WriteLine();

            myLogger.LogInformation("Console output plugin initialized");
            await Task.CompletedTask;
        }

        public async Task ProcessMetricsAsync(SystemMetrics metrics, CancellationToken cancellationToken = default)
        {
            try
            {
                // Output with color coding based on usage levels
                Console.ForegroundColor = GetColorForCpuUsage(metrics.CpuUsagePercent);
                Console.WriteLine(metrics.ToString());
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                myLogger.LogError(ex, "Failed to output metrics to console");
            }

            await Task.CompletedTask;
        }

        public async Task CleanupAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine();
            Console.WriteLine("=== System Monitor Console Output Stopped ===");

            myLogger.LogInformation("Console output plugin cleaned up");
            await Task.CompletedTask;
        }

        private static ConsoleColor GetColorForCpuUsage(double cpuUsage)
        {
            return cpuUsage switch
            {
                >= 80 => ConsoleColor.Red,
                >= 60 => ConsoleColor.Yellow,
                >= 40 => ConsoleColor.DarkYellow,
                _ => ConsoleColor.Green
            };
        }
    }
}

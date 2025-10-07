using Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace Services
{
    /// <summary>
    /// Background service that orchestrates system monitoring
    /// </summary>
    public class MonitoringService : BackgroundService
    {
        private readonly ISystemMonitor mySystemMonitor;
        private readonly IPluginManager myPluginManager;
        private readonly ILogger<MonitoringService> myLogger;
        private readonly int myIntervalSeconds;

        public MonitoringService(
            ISystemMonitor systemMonitor,
            IPluginManager pluginManager,
            ILogger<MonitoringService> logger, 
            int intervalSeconds)
        {
            mySystemMonitor = systemMonitor ?? throw new ArgumentNullException(nameof(systemMonitor));
            myPluginManager = pluginManager ?? throw new ArgumentNullException(nameof(pluginManager));
            myLogger = logger ?? throw new ArgumentNullException(nameof(logger));
            myIntervalSeconds = intervalSeconds > 0 ? intervalSeconds : throw new ArgumentOutOfRangeException(nameof(intervalSeconds), "Interval must be greater than zero");
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            myLogger.LogInformation("Starting System Monitor Service");

            try
            {
                // Initialize all plugins
                await myPluginManager.InitializeAllPluginsAsync(cancellationToken);
                myLogger.LogInformation("System Monitor Service started successfully");
            }
            catch (Exception ex)
            {
                myLogger.LogError(ex, "Failed to start System Monitor Service");
                throw;
            }

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            myLogger.LogInformation("System monitoring started with interval: {IntervalSeconds} seconds", myIntervalSeconds);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        // Collect system metrics
                        var metrics = await mySystemMonitor.GetSystemMetricsAsync();

                        // Process metrics through all plugins
                        await myPluginManager.ProcessMetricsThroughPluginsAsync(metrics, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        myLogger.LogError(ex, "Error during monitoring cycle");
                    }

                    // Wait for the configured interval
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(myIntervalSeconds), stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected when cancellation is requested
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                myLogger.LogInformation("System monitoring stopped due to cancellation");
            }
            catch (Exception ex)
            {
                myLogger.LogError(ex, "Unexpected error in monitoring service");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            myLogger.LogInformation("Stopping System Monitor Service");

            try
            {
                // Cleanup all plugins
                await myPluginManager.CleanupAllPluginsAsync(cancellationToken);
                myLogger.LogInformation("System Monitor Service stopped successfully");
            }
            catch (Exception ex)
            {
                myLogger.LogError(ex, "Error during service shutdown");
            }

            await base.StopAsync(cancellationToken);
        }
    }
}

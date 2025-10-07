using Core.Interfaces;
using Core.Models;
using Microsoft.Extensions.Logging;

namespace Impl
{
    /// <summary>
    /// Manages and orchestrates monitor plugins
    /// </summary>
    public class PluginManager : IPluginManager
    {
        private readonly List<IMonitorPlugin> myPlugins;
        private readonly ILogger<PluginManager> myLogger;

        public PluginManager(ILogger<PluginManager> logger)
        {
            myLogger = logger ?? throw new ArgumentNullException(nameof(logger));
            myPlugins = new List<IMonitorPlugin>();
        }

        public void RegisterPlugin(IMonitorPlugin plugin)
        {
            if (plugin == null)
                throw new ArgumentNullException(nameof(plugin));

            myPlugins.Add(plugin);
            myLogger.LogInformation("Registered plugin: {PluginName}", plugin.Name);
        }

        public async Task InitializeAllPluginsAsync(CancellationToken cancellationToken = default)
        {
            myLogger.LogInformation("Initializing {PluginCount} plugins", myPlugins.Count);

            var tasks = myPlugins.Select(async plugin =>
            {
                try
                {
                    await plugin.InitializeAsync(cancellationToken);
                    myLogger.LogInformation("Successfully initialized plugin: {PluginName}", plugin.Name);
                }
                catch (Exception ex)
                {
                    myLogger.LogError(ex, "Failed to initialize plugin: {PluginName}", plugin.Name);
                }
            });

            await Task.WhenAll(tasks);
        }

        public async Task ProcessMetricsThroughPluginsAsync(SystemMetrics metrics, CancellationToken cancellationToken = default)
        {
            if (metrics == null)
                throw new ArgumentNullException(nameof(metrics));

            var tasks = myPlugins.Select(async plugin =>
            {
                try
                {
                    await plugin.ProcessMetricsAsync(metrics, cancellationToken);
                }
                catch (Exception ex)
                {
                    myLogger.LogError(ex, "Plugin {PluginName} failed to process metrics", plugin.Name);
                }
            });

            await Task.WhenAll(tasks);
        }

        public async Task CleanupAllPluginsAsync(CancellationToken cancellationToken = default)
        {
            myLogger.LogInformation("Cleaning up {PluginCount} plugins", myPlugins.Count);

            var tasks = myPlugins.Select(async plugin =>
            {
                try
                {
                    await plugin.CleanupAsync(cancellationToken);
                    myLogger.LogInformation("Successfully cleaned up plugin: {PluginName}", plugin.Name);
                }
                catch (Exception ex)
                {
                    myLogger.LogError(ex, "Failed to cleanup plugin: {PluginName}", plugin.Name);
                }
            });

            await Task.WhenAll(tasks);
        }
    }
}

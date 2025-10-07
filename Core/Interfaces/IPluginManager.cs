using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    /// <summary>
    /// Service for managing and executing monitor plugins
    /// </summary>
    public interface IPluginManager
    {
        /// <summary>
        /// Register a plugin with the manager
        /// </summary>
        /// <param name="plugin">Plugin to register</param>
        void RegisterPlugin(IMonitorPlugin plugin);

        /// <summary>
        /// Initialize all registered plugins
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task InitializeAllPluginsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Process metrics through all registered plugins
        /// </summary>
        /// <param name="metrics">System metrics to process</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ProcessMetricsThroughPluginsAsync(Core.Models.SystemMetrics metrics, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cleanup all registered plugins
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task CleanupAllPluginsAsync(CancellationToken cancellationToken = default);
    }
}

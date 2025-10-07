using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    /// <summary>
    /// Plugin interface for extending monitoring functionality
    /// </summary>
    public interface IMonitorPlugin
    {
        /// <summary>
        /// Plugin name for identification
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Called when new system metrics are available
        /// </summary>
        /// <param name="metrics">The collected system metrics</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ProcessMetricsAsync(SystemMetrics metrics, CancellationToken cancellationToken = default);

        /// <summary>
        /// Initialize the plugin (called once at startup)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Cleanup plugin resources (called at shutdown)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task CleanupAsync(CancellationToken cancellationToken = default);
    }
}

using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    /// <summary>
    /// Interface for system resource monitoring implementations
    /// </summary>
    public interface ISystemMonitor
    {
        /// <summary>
        /// Gets current system metrics
        /// </summary>
        /// <returns>System metrics including CPU, RAM, and disk usage</returns>
        Task<SystemMetrics> GetSystemMetricsAsync();
    }
}

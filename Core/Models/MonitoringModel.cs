using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class MonitoringSettings
    {
        public int IntervalSeconds { get; set; } = 5;
        public string ApiEndpoint { get; set; } = string.Empty;
        public bool EnableConsoleOutput { get; set; } = true;
        public bool EnableFileLogging { get; set; } = true;
        public string LogFilePath { get; set; } = "system-monitor.log";
    }
}

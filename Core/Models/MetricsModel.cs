using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class SystemMetrics
    {
        public double CpuUsagePercent { get; set; }
        public long RamUsedMb { get; set; }
        public long RamTotalMb { get; set; }
        public long DiskUsedMb { get; set; }
        public long DiskTotalMb { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] CPU: {CpuUsagePercent:F1}% | " +
                   $"RAM: {RamUsedMb:N0}/{RamTotalMb:N0} MB ({GetPercentage(RamUsedMb, RamTotalMb):F1}%) | " +
                   $"Disk: {DiskUsedMb:N0}/{DiskTotalMb:N0} MB ({GetPercentage(DiskUsedMb, DiskTotalMb):F1}%)";
        }

        private static double GetPercentage(long used, long total)
        {
            return total > 0 ? (double)used / total * 100 : 0;
        }
    }
}

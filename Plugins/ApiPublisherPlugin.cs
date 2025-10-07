using Core.Interfaces;
using Microsoft.Extensions.Logging;

using System.Text;

using Core.Models;

using Newtonsoft.Json;


namespace Plugins
{
    /// <summary>
    /// Plugin that sends metrics to a REST API endpoint
    /// </summary>
    public class ApiPublisherPlugin : IMonitorPlugin
    {
        private readonly ILogger<ApiPublisherPlugin> myLogger;
        private readonly HttpClient myHttpClient;
        private readonly string myApiEndpoint;

        public string Name => "API Publisher Plugin";

        public ApiPublisherPlugin(ILogger<ApiPublisherPlugin> logger, string apiEndpoint, HttpClient? httpClient = null)
        {
            myLogger = logger ?? throw new ArgumentNullException(nameof(logger));
            myApiEndpoint = apiEndpoint ?? throw new ArgumentNullException(nameof(apiEndpoint));
            myHttpClient = httpClient ?? new HttpClient();

            // Configure HTTP client
            myHttpClient.DefaultRequestHeaders.Add("User-Agent", "SystemMonitor/1.0");
            myHttpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(myApiEndpoint)) return Task.CompletedTask;

                myLogger.LogWarning("API endpoint is not configured. Plugin will be disabled.");
                return Task.FromException(new ArgumentNullException(myApiEndpoint, "Api endpoint is null or empty"));
            }
            catch (Exception ex)
            {
                myLogger.LogError(ex, "Failed to initialize API publisher plugin");
                throw;
            }
        }

        public async Task ProcessMetricsAsync(SystemMetrics metrics, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(myApiEndpoint))
                {
                    return; // Plugin is disabled
                }

                // Create JSON payload 
                var payload = new
                {
                    cpu = Math.Round(metrics.CpuUsagePercent, 2),
                    ram_used = metrics.RamUsedMb,
                    disk_used = metrics.DiskUsedMb,
                    timestamp = metrics.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                };

                var json = JsonConvert.SerializeObject(payload, Formatting.None);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                myLogger.LogDebug("Sending metrics to API: {Json}", json);

                var response = await myHttpClient.PostAsync(myApiEndpoint, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    myLogger.LogDebug("Successfully sent metrics to API endpoint");
                }
                else
                {
                    myLogger.LogWarning("API request failed with status code: {StatusCode}", response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                myLogger.LogError(ex, "HTTP error while sending metrics to API");
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                myLogger.LogError("Timeout while sending metrics to API");
            }
            catch (Exception ex)
            {
                myLogger.LogError(ex, "Unexpected error while sending metrics to API");
            }
        }

        public Task CleanupAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                myHttpClient.Dispose();
                myLogger.LogInformation("API publisher plugin cleaned up");
            }
            catch (Exception ex)
            {
                myLogger.LogError(ex, "Failed to cleanup API publisher plugin");
            }

            return Task.CompletedTask;
        }
    }
}

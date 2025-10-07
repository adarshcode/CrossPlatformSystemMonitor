using Core.Interfaces;
using Core.Models;

using Impl;
using Impl.Monitors;
using Plugins;
using Services;


using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Cross-Platform System Monitor");
        Console.WriteLine("=========================================================");
        Console.WriteLine();

        try
        {
            // Build and run the host
            var host = CreateHostBuilder(args).Build();

            // Handle Ctrl+C gracefully
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                Console.WriteLine("\n Cancelling requested. Stopping gracefully...");
                host.Services.GetRequiredService<IHostApplicationLifetime>().StopApplication();
            };

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>

        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appSettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Register core services
                services.AddSingleton<ISystemMonitor, SystemMonitor>();
                services.AddSingleton<IPluginManager, PluginManager>();

                // Register plugins
                services.AddSingleton<IMonitorPlugin>(provider =>
                {
                    var logger = provider.GetRequiredService<ILogger<ConsoleOutputPlugin>>();
                    return new ConsoleOutputPlugin(logger);
                });

                services.AddSingleton<IMonitorPlugin>(provider =>
                {
                    var logger = provider.GetRequiredService<ILogger<LoggingPlugin>>();
                    var settings = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<MonitoringSettings>>().Value;
                    return new LoggingPlugin(logger, settings.LogFilePath);
                });

                services.AddSingleton<IMonitorPlugin>(provider =>
                {
                    var logger = provider.GetRequiredService<ILogger<ApiPublisherPlugin>>();
                    var settings = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<MonitoringSettings>>().Value;
                    return new ApiPublisherPlugin(logger, settings.ApiEndpoint);
                });

                // Configure logging
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddConfiguration(context.Configuration.GetSection("Logging"));
                });
            })
            .ConfigureServices((context, services) =>
            {
                // Configure plugin registration
                services.AddSingleton<IHostedService>(provider =>
                {
                    var systemMonitor = provider.GetRequiredService<ISystemMonitor>();
                    var pluginManager = provider.GetRequiredService<IPluginManager>();
                    var logger = provider.GetRequiredService<ILogger<MonitoringService>>();
                    var settings = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<MonitoringSettings>>();

                    // Register plugins with the plugin manager
                    var plugins = provider.GetServices<IMonitorPlugin>();
                    foreach (var plugin in plugins)
                    {
                        pluginManager.RegisterPlugin(plugin);
                    }

                    return new MonitoringService(systemMonitor, pluginManager, logger, settings.Value.IntervalSeconds);
                });
            });
}
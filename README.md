# Cross-Platform System Monitor

A .NET 8 based cross-platform system monitoring application that collects and processes system metrics through an extensible plugin architecture.

## Prerequisites

- **.NET 8.0 SDK** or later
- **Windows OS** (for current implementation)
- **Administrator privileges** (recommended for performance counter access)

## How to Build

### Using Visual Studio
1. Open `CrossPlatformSystemMonitor.sln` in Visual Studio 2022
2. Build the solution using `Build > Build Solution` or `Ctrl+Shift+B`

### Using .NET CLI
```powershell
# Navigate to the solution directory
cd "\CrossPlatformSystemMonitor"

# Restore dependencies
dotnet restore CrossPlatformSystemMonitor/CrossPlatformSystemMonitor.sln

# Build the solution
dotnet build CrossPlatformSystemMonitor/CrossPlatformSystemMonitor.sln --configuration Release
```

## How to Run

### From Visual Studio
1. Set `CrossPlatformSystemMonitor` as the startup project
2. Press `F5` to run with debugging or `Ctrl+F5` to run without debugging

### Using .NET CLI
```powershell
# Run from the main project directory
cd "\CrossPlatformSystemMonitor\CrossPlatformSystemMonitor"
dotnet run

# Or run the built executable
dotnet run --configuration Release
```

### Using the Executable
```powershell
# Navigate to the output directory
cd "CrossPlatformSystemMonitor\CrossPlatformSystemMonitor\bin\Release\net8.0"

# Run the executable
.\CrossPlatformSystemMonitor.exe
```

## Overview

This application monitors system resources (CPU, RAM, and Disk usage) and processes the collected metrics through configurable plugins. It's designed with a modular architecture to support different platforms and extensible output mechanisms.

## Features

- **Cross-platform monitoring**: Currently supports Windows with Linux/macOS support planned
- **Real-time metrics collection**: CPU usage, RAM utilization, and disk space monitoring
- **Plugin-based architecture**: Extensible system for custom metric processing
- **Multiple output formats**: Console output, file logging, and API publishing
- **Configurable monitoring intervals**: Adjustable collection frequency
- **Graceful shutdown**: Proper resource cleanup and cancellation handling

## Configuration

The application is configured through `appSettings.json`:

```json
{
  "MonitoringSettings": {
    "IntervalSeconds": 5,                           // Metrics collection interval
    "ApiEndpoint": "https://api.example.com/metrics", // REST API endpoint for publishing
    "EnableConsoleOutput": true,                    // Enable console output plugin
    "EnableFileLogging": true,                      // Enable file logging plugin
    "LogFilePath": "system-monitor.log"             // Log file path
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

## Architecture Decisions

### Design Philosophy

The application follows **Clean Architecture** principles with **Dependency Inversion** and **Plugin Pattern** to achieve:

1. **Separation of Concerns**: Each layer has distinct responsibilities
2. **Platform Abstraction**: Core logic is independent of platform-specific implementations
3. **Extensibility**: New plugins can be added without modifying existing code
4. **Testability**: Dependencies are injected and interfaces are mocked easily
5. **Maintainability**: Changes in one layer don't cascade to others

### Key Design Patterns

- **Strategy Pattern**: Different monitoring implementations per platform
- **Plugin Pattern**: Extensible metric processing through `IMonitorPlugin`
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection for IoC
- **Background Service**: Long-running monitoring using `BackgroundService`
- **Factory Pattern**: Platform-specific monitor creation in `SystemMonitor`

## Data Flow

1. `MonitoringService` orchestrates the monitoring loop
2. `SystemMonitor` collects platform-specific metrics via `WindowsSystemMonitor`
3. `PluginManager` distributes metrics to all registered plugins
4. Each plugin processes metrics according to its purpose (console, file, API)

## Limitations & Challenges

### Platform Support
- **Current**: Only Windows implementation using PerformanceCounters
- **Challenge**: Linux/macOS require different APIs (proc filesystem, system calls)

### Error Handling
- **Current**: Basic exception logging with continuation
- **Challenge**: Transient network errors in API plugin could cause data loss

### Configuration Management
- **Current**: Static JSON configuration
- **Challenge**: Runtime configuration changes require application restart

### Plugin Lifecycle
- **Current**: All plugins are initialized at startup
- **Challenge**: Plugin failures can affect the entire monitoring process

## Dependencies

- **Microsoft.Extensions.Hosting**: Background service infrastructure
- **Microsoft.Extensions.Logging**: Structured logging
- **Newtonsoft.Json**: JSON serialization for API plugin
- **System.Diagnostics.PerformanceCounter**: Windows performance counters
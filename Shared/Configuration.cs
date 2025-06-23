using Drv.ChrDrvSettings;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Spectre;
using Spectre.Console;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;

namespace Shared;

public static class Configuration
{
    public static void ConfigureLogger()
    {
        AnsiConsole.MarkupLine("Настраиваю логгер...".MarkupYellow());
        
        const string logs = "logs";
        const string outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}";
        
        var logsPath = Path.Combine(logs);
        Directory.CreateDirectory(logsPath);
        
        var levelSwitch = new LoggingLevelSwitch();
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(levelSwitch)
            .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .WriteTo.Spectre(outputTemplate: outputTemplate)
            .WriteTo.File($"{logsPath}/.log", rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate, levelSwitch: levelSwitch)
            .CreateLogger();
    }

    public static void ConfigureDriver(IServiceCollection services, string chromePath = "Chrome")
    {
        AnsiConsole.MarkupLine("Настраиваю драйвер...".MarkupYellow());
        
        var chromeDirectory = Path.Combine(Directory.GetCurrentDirectory(), chromePath);

        Directory.CreateDirectory(chromePath);
        
        var driverManager = new DriverManager();
        var driverPath = driverManager.SetUpDriver(new ChromeConfig(), VersionResolveStrategy.MatchingBrowser);
        
        if (!File.Exists(driverPath)) throw new ApplicationException("Ошибка настройки драйвера");
        
        services.AddSingleton<ChrDrvSettingsWithoutDriver>(_ => new ChrDrvSettingsWithoutDriver
        {
            ChromeDir = chromeDirectory,
            UsernameDir = "RealUser",
            DriverPath = driverPath
        });
    }
}
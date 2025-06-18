using Drv.ChrDrvSettings;
using Serilog;
using Serilog.Events;
using Spectre.Console;

namespace Shared;

public static class Configuration
{
    public static readonly ChrDrvSettingsBase DrvSettings = new ChrDrvSettingsWithAutoDriver()
    {
        ChromeDir = @"D:\Chrome",
        UsernameDir = "NewUser"
    };

    static Configuration()
    {
        AnsiConsole.MarkupLine("Запуск...".MarkupSecondary());
        Configure();
    }

    private static void Configure()
    {
        const string outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}";
        var logsPath = Path.Combine("logs");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: outputTemplate)
            .WriteTo.File($"{logsPath}/.log", rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate, restrictedToMinimumLevel: LogEventLevel.Error)
            .CreateLogger();
    }
}
using Serilog;
using Serilog.Core;
using Serilog.Sinks.Spectre;
using Spectre.Console;

namespace Shared;

public static class Configuration
{
    static Configuration()
    {
        AnsiConsole.MarkupLine("Запуск...".MarkupSecondary());
        Configure();
    }

    public static void Configure()
    {
        const string logs = "logs";
        const string outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}";
        var logsPath = Path.Combine(logs);
        Directory.CreateDirectory(logsPath);
        var levelSwitch = new LoggingLevelSwitch();
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(levelSwitch)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .WriteTo.Spectre(outputTemplate: outputTemplate)
            .WriteTo.File($"{logsPath}/.log", rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate, levelSwitch: levelSwitch)
            .CreateLogger();
    }
}
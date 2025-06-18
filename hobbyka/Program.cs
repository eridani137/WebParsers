using Drv;
using Drv.ChrDrvSettings;
using Serilog;
using Serilog.Events;

const string outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}";
var logsPath = Path.Combine("logs");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: outputTemplate)
    .WriteTo.File($"{logsPath}/.log", rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate, restrictedToMinimumLevel: LogEventLevel.Error)
    .CreateLogger();

var drvSettings = new ChrDrvSettingsWithAutoDriver()
{
    ChromeDir = @"D:\Chrome",
    UsernameDir = "NewUser"
};
using var drv = await ChrDrvFactory.Create(drvSettings);

drv.Navigate().GoToUrl("https://nowsecure.nl");

Console.ReadKey(true);
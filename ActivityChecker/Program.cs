using ActivityChecker;
using Drv.ChrDrvSettings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Serilog;
using Shared;

try
{
    Configuration.Configure();
    var builder = Host.CreateApplicationBuilder();
    
    var appSettings = builder.Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();
    if (appSettings is null) throw new ApplicationException("Не найдена конфигурация");

    builder.Services.AddSerilog();
    builder.Services.AddSingleton<ChrDrvSettingsWithAutoDriver>(_ => new ChrDrvSettingsWithAutoDriver
    {
        ChromeDir = appSettings.ChromeDir,
        UsernameDir = "RealUser"
    });
    
    builder.Services.AddSingleton<IMongoClient>(_ =>
        new MongoClient(builder.Configuration.GetConnectionString("Mongo")));

    builder.Services.AddSingleton<CheckerService>();

    builder.Services.AddHostedService<ConsoleMenu>();

    var app = builder.Build();
    
    AppDomain.CurrentDomain.ProcessExit += (_, _) =>
    {
        Drv.Extensions.KillAllOpenedBrowsers();
    };
    
    await app.RunAsync();
}
catch (Exception e)
{
    Log.Fatal(e, "Приложение смогло запуститься");
}
finally
{
    await Log.CloseAndFlushAsync();
}
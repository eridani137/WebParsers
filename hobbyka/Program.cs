using Drv.ChrDrvSettings;
using hobbyka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

try
{
    var builder = Host.CreateApplicationBuilder();

    builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(nameof(AppSettings)));
    builder.Services.AddSerilog();
    builder.Services.AddSingleton<ChrDrvSettingsWithAutoDriver>(_ => new ChrDrvSettingsWithAutoDriver
    {
        ChromeDir = @"D:\Chrome",
        UsernameDir = "NewUser"
    });
    builder.Services.AddHostedService<Parser>();

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
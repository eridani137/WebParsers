using Drv.ChrDrvSettings;
using hobbyka;
using hobbyka.WooCommerce;
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

    builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(nameof(AppSettings)));
    builder.Services.AddSerilog();
    builder.Services.AddSingleton<ChrDrvSettingsWithAutoDriver>(_ => new ChrDrvSettingsWithAutoDriver
    {
        ChromeDir = @"D:\Chrome",
        UsernameDir = "NewUser"
    });
    
    builder.Services.AddSingleton<IMongoClient>(_ =>
        new MongoClient(builder.Configuration.GetConnectionString("Mongo")));
    
    builder.Services.AddSingleton<WooCommerceExporter>();
    builder.Services.AddSingleton<Parser>();
    builder.Services.AddSingleton<Exporter>();
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
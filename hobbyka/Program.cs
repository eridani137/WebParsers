using Drv.ChrDrvSettings;
using hobbyka;
using hobbyka.WooCommerce;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
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
    var config = builder.Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();
    if (config is null) throw new Exception("config is null");
    if (config.Export.Count > 0)
    {
        builder.Services.AddHostedService<Exporter>();
    }
    else
    {
        builder.Services.AddHostedService<Parser>();
    }
    builder.Services.AddSingleton<IMongoClient>(_ =>
        new MongoClient(builder.Configuration.GetConnectionString("Mongo")));
    builder.Services.AddSingleton<WooCommerceExporter>();

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
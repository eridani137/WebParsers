using ActivityChecker;
using ActivityChecker.IO;
using ActivityChecker.Services;
using Drv.ChrDrvSettings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Shared;
using VkNet.Model;

try
{
    Configuration.ConfigureLogger();
    var builder = Host.CreateApplicationBuilder();

    const string chromeDir = "Chrome";
    
    Configuration.ConfigureDriver(builder.Services, chromeDir);

    builder.Services.AddSerilog();
    builder.Services.AddSingleton<ChrDrvSettingsWithAutoDriver>(_ => new ChrDrvSettingsWithAutoDriver
    {
        ChromeDir = chromeDir,
        UsernameDir = "RealUser"
    });

    var vkApi = new VkNet.VkApi(builder.Services);
    await vkApi.AuthorizeAsync(new ApiAuthParams()
    {
        AccessToken = "091f3a59091f3a59091f3a59a80a2bf8470091f091f3a596147cf3f7b83617957730c3e"
    });
    builder.Services.AddSingleton(vkApi);
    
    builder.Services.AddParsers();
    builder.Services.AddSingleton<CheckerService>();
    builder.Services.AddSingleton<CsvExporter>();
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
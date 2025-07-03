using ashoo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Serilog;
using Shared;

try
{
    Configuration.ConfigureLogger();
    var builder = Host.CreateApplicationBuilder();
    
    builder.Services.AddSerilog();
    builder.Services.AddSingleton<IMongoClient>(_ =>
        new MongoClient(builder.Configuration.GetConnectionString("Mongo")));

    builder.Services.AddSingleton<Exporter>();
    builder.Services.AddHostedService<ConsoleMenu>();
    
    var app = builder.Build();
    
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
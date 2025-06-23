using System.Reflection;
using ActivityChecker.Parsers;
using Microsoft.Extensions.DependencyInjection;

namespace ActivityChecker;

public static class ServiceExtensions
{
    public static IServiceCollection AddParsers(this IServiceCollection services)
    {
        services.AddSingleton<ParserFactory>();

        var assembly = Assembly.GetExecutingAssembly();
        var parserTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ISiteParser).IsAssignableFrom(t));

        foreach (var parserType in parserTypes)
        {
            services.AddSingleton(typeof(ISiteParser), parserType);
        }
        
        return services;
    }
}
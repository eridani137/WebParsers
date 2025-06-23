using Microsoft.Extensions.DependencyInjection;

namespace ActivityChecker.Parsers;

public class ParserFactory(IServiceProvider provider)
{
    public ISiteParser? GetParser(string url)
    {
        return provider.GetServices<ISiteParser>()
            .FirstOrDefault(p => url.StartsWith(p.Url));
    }

    public T GetParser<T>()  where T : ISiteParser
    {
        return (T)GetParser(typeof(T).Name)!;
    }
}
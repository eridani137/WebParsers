using Microsoft.Extensions.Hosting;
using Shared;
using Shared.Menu;

namespace hobbyka;

public class ConsoleMenu(IHostApplicationLifetime lifetime, Parser parser, Exporter exporter)
    : BaseConsoleMenu(lifetime)
{
    protected override List<MenuItem> MenuItems { get; } =
    [
        new("Парсинг", parser.ExecuteAsync),
        new("Экспорт", exporter.ExecuteAsync)
    ];
}
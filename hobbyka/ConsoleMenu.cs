using Microsoft.Extensions.Hosting;
using Shared;
using Shared.Menu;

namespace hobbyka;

public class ConsoleMenu(IHostApplicationLifetime lifetime, Parser parser, Exporter exporter)
    : BaseConsoleMenu(lifetime)
{
    protected override List<BaseMenuItem> MenuItems { get; } =
    [
        new MenuItem("Парсинг", parser.ExecuteAsync),
        new MenuItem("Экспорт", exporter.ExecuteAsync)
    ];
}
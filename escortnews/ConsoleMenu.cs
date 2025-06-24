using Microsoft.Extensions.Hosting;
using Shared.Menu;

namespace escortnews;

public class ConsoleMenu(IHostApplicationLifetime lifetime, Parser parser) : BaseConsoleMenu(lifetime)
{
    protected override List<BaseMenuItem> MenuItems { get; } =
    [
        new MenuItem("Parse", parser.Parse),
        new MenuItem("Export", parser.Export)
    ];
}
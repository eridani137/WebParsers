using Microsoft.Extensions.Hosting;
using Shared.Menu;

namespace ashoo;

public class ConsoleMenu(IHostApplicationLifetime lifetime, Exporter exporter) : BaseConsoleMenu(lifetime)
{
    protected override List<BaseMenuItem> MenuItems { get; } =
    [
        new MenuItem("Export", exporter.Export)
    ];
}
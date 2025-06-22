using Microsoft.Extensions.Hosting;
using Shared;

namespace ActivityChecker;

public class ConsoleMenu(IHostApplicationLifetime lifetime, CheckerService checkerService) : BaseConsoleMenu(lifetime)
{
    protected override List<MenuItem> MenuItems { get; } = 
    [
        new("Открыть файл", checkerService.CheckFile)
    ];
}
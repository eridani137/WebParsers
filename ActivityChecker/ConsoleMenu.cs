using ActivityChecker.Parsers;
using ActivityChecker.Services;
using Microsoft.Extensions.Hosting;
using Shared;
using Shared.Menu;
using Spectre.Console;

namespace ActivityChecker;

public class ConsoleMenu(IHostApplicationLifetime lifetime, CheckerService checkerService) : BaseConsoleMenu(lifetime)
{
    protected override List<BaseMenuItem> MenuItems { get; } =
    [
        new SubMenuItem("Открыть файл", [
            new MenuItem("VK", async () =>
            {
                await checkerService.CheckFile(typeof(VkParser));
            }),
            new MenuItem("Youtube", () =>
            {
                AnsiConsole.MarkupLine("Не реализовано".MarkupRed());
                return Task.CompletedTask;
            }),
            new MenuItem("Instagram", () =>
            {
                AnsiConsole.MarkupLine("Не реализовано".MarkupRed());
                return Task.CompletedTask;
            })
        ]),
        new MenuItem("Авторизация", checkerService.Authorization)
    ];
}
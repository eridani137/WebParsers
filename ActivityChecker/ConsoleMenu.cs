using System.Diagnostics;
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
        new SubMenuItem("Открыть файл с ссылками", [
            new MenuItem("VK", async () =>
            {
                await checkerService.CheckFile();
            }),
            new MenuItem("Youtube", async () =>
            {
                await checkerService.CheckFile();
            }),
            new MenuItem("Instagram", async () =>
            {
                await checkerService.CheckFile();
            })
        ]),
        new MenuItem("Открыть директорию с результатами", () =>
        {
            Directory.CreateDirectory(CheckerService.ResultDirectory);
            Process.Start(new ProcessStartInfo(CheckerService.ResultDirectory) { UseShellExecute = true });
            return Task.CompletedTask;
        }),
        new MenuItem("Авторизация", checkerService.Authorization)
    ];
}
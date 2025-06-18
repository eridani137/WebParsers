using Drv;
using Shared;
using Spectre.Console;

using var drv = await ChrDrvFactory.Create(Configuration.DrvSettings);

drv.Navigate().GoToUrl("https://nowsecure.nl");

AnsiConsole.MarkupLine("Нажмите любую клавишу для выхода...".MarkupSecondary());
Console.ReadKey(true);
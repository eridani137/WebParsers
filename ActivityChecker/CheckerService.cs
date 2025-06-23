using ActivityChecker.Parsers;
using Drv;
using Drv.ChrDrvSettings;
using Shared;
using Spectre.Console;
using Extensions = Shared.Extensions;

namespace ActivityChecker;

public class CheckerService(ChrDrvSettingsWithoutDriver drvSettings, ParserFactory parserFactory)
{
    public async Task CheckFile(Type type)
    {
        var input = new PathUserInput();
        var lines = await File.ReadAllLinesAsync(input.Path.Trim('"'));
        
        AnsiConsole.MarkupLine($"Прочитано {lines.Length} строк".Highlight(SpectreConfig.Fuchsia, SpectreConfig.Aquamarine));

        if (type == typeof(VkParser))
        {
            var parser = parserFactory.GetParser<VkParser>();
            parser.GetViewCount(lines);
        }
        
        AnsiConsole.MarkupLine("Все ссылки обработаны".MarkupAqua());
    }

    public async Task Authorization()
    {
        AnsiConsole.MarkupLine("Пройдите авторизацию и нажмите любую клавишу...".MarkupAqua());
        
        using var drv = await ChrDrvFactory.Create(drvSettings);

        Console.ReadKey(true);
    }
}
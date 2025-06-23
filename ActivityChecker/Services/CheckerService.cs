using System.Collections.Immutable;
using ActivityChecker.IO;
using ActivityChecker.Parsers;
using Drv;
using Drv.ChrDrvSettings;
using Shared;
using Spectre.Console;

namespace ActivityChecker.Services;

public class CheckerService(ChrDrvSettingsWithoutDriver drvSettings, ParserFactory parserFactory, CsvExporter exporter)
{
    public async Task CheckFile(Type type)
    {
        const string results = "results";
        Directory.CreateDirectory(results);
        
        var input = new PathUserInput();
        var lines = (await File.ReadAllLinesAsync(input.Path.Trim('"'))).ToImmutableList();
        
        AnsiConsole.MarkupLine($"Прочитано {lines.Count} строк".Highlight(SpectreConfig.Fuchsia, SpectreConfig.Aquamarine));

        if (type == typeof(VkParser))
        {
            var parser = parserFactory.GetParser(lines.First());
            if (parser is not null)
            {
                var result = await parser.GetViewCount(lines);

                AnsiConsole.MarkupLine("Все ссылки обработаны".MarkupAqua());
                
                await exporter.Export(Path.Join(results, $"VK_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv"), result);
            }
            else throw new ApplicationException("parser not found");
        }
        
        AnsiConsole.MarkupLine("Операция завершена".MarkupAqua());
    }

    public async Task Authorization()
    {
        AnsiConsole.MarkupLine("Пройдите авторизацию и нажмите любую клавишу...".MarkupAqua());
        
        using var drv = await ChrDrvFactory.Create(drvSettings);

        Console.ReadKey(true);
    }
}
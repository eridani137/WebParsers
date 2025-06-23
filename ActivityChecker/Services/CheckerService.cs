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
    public async Task CheckFile()
    {
        const string results = "results";
        Directory.CreateDirectory(results);

        var input = new PathUserInput();
        var lines = (await File.ReadAllLinesAsync(input.Path.Trim('"'))).ToImmutableList();

        AnsiConsole.MarkupLine(
            $"Прочитано {lines.Count} строк".Highlight(SpectreConfig.Fuchsia, SpectreConfig.Aquamarine));

        var parser = parserFactory.GetParser(lines.First());
        if (parser is null) throw new ApplicationException("parser not found");

        var result = await parser.GetViewCount(lines);

        if (result.Count > 0)
        {
            AnsiConsole.MarkupLine("Все ссылки обработаны".MarkupAqua());

            var exportPath = Path.Join(results, $"{parser.GetType().Name.Replace("Parser", "")}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv");
            
            await exporter.Export(exportPath, result);

            AnsiConsole.MarkupLine("Операция завершена".MarkupAqua());
            AnsiConsole.Write(new TextPath(exportPath.EscapeMarkup())
                .RootColor(Color.Yellow)
                .SeparatorColor(Color.SeaGreen1)
                .StemColor(Color.Yellow)
                .LeafColor(Color.Green));
            AnsiConsole.WriteLine();
        }
        else
        {
            AnsiConsole.MarkupLine("Нет данных для сохранения".MarkupRed());
        }
    }

    public async Task Authorization()
    {
        AnsiConsole.MarkupLine("Пройдите авторизацию и нажмите любую клавишу...".MarkupAqua());

        using var drv = await ChrDrvFactory.Create(drvSettings);

        Console.ReadKey(true);
    }
}
using System.Collections.Immutable;
using System.Text.Json;
using ActivityChecker.IO;
using Drv;
using Drv.ChrDrvSettings;
using Drv.Stealth.Clients.Extensions;
using Microsoft.Extensions.Logging;
using ParserExtension;
using Spectre.Console;

namespace ActivityChecker.Parsers;

public class InstagramParser(ChrDrvSettingsWithAutoDriver drvSettings, ILogger<InstagramParser> logger) :  ISiteParser
{
    public string Url => "https://www.instagram.com";
    
    public async Task<List<ViewResult>> GetViewCount(ImmutableList<string> lines)
    {
        var result = new List<ViewResult>();

        using var drv = await ChrDrvFactory.Create(drvSettings);

        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("Получение страниц...", maxValue: lines.Count);

                foreach (var line in lines)
                {
                    try
                    {
                        await drv.Navigate().GoToUrlAsync(line);
                        drv.SpecialWait(1500);
                        var parse = drv.PageSource.GetParse();
                        if (parse is null) throw new ApplicationException("parse is null");

                        var viewCountStr =
                            parse.GetInnerText(
                                "//main[@role='main']//span[contains(text(), 'отметок')]/span[contains(@class, 'html-span')]/text()");
                        if (viewCountStr is null || !int.TryParse(viewCountStr.Replace(" ",""), out var viewCount)) throw new ApplicationException("Не удалось получить количество просмотров");

                        result.Add(new ViewResult()
                        {
                            Url = line,
                            Views = viewCount
                        });
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Ошибка в цикле обработки ссылок");
                    }
                    finally
                    {
                        task.Increment(1);
                    }
                }
            });

        return result;
    }
}
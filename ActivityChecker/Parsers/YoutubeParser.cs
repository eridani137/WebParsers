using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;
using ActivityChecker.IO;
using Drv;
using Drv.ChrDrvSettings;
using Microsoft.Extensions.Logging;
using ParserExtension;
using Spectre.Console;

namespace ActivityChecker.Parsers;

public class YoutubeParser(ChrDrvSettingsWithAutoDriver drvSettings, ILogger<VkParser> logger) : ISiteParser
{
    public string Url => "https://youtube.com";

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
                        var parse = drv.PageSource.GetParse();
                        if (parse is null) throw new ApplicationException("parse is null");

                        var script = parse.GetInnerHtml("//script[@nonce and contains(text(), 'var ytInitialData')]");
                        if (script is null) throw new ApplicationException("script is null");

                        var trim = script.Remove(0, 20);
                        trim = trim.Remove(trim.Length - 1, 1);

                        using var document = JsonDocument.Parse(trim);
                        var viewCountElement = document.RootElement
                            .GetProperty("contents")
                            .GetProperty("twoColumnWatchNextResults")
                            .GetProperty("results")
                            .GetProperty("results")
                            .GetProperty("contents")[0]
                            .GetProperty("videoPrimaryInfoRenderer")
                            .GetProperty("viewCount")
                            .GetProperty("videoViewCountRenderer")
                            .GetProperty("viewCount")
                            .GetProperty("simpleText")
                            .GetString();

                        if (viewCountElement is null)
                            throw new ApplicationException("Не удалось получить количество просмотров");

                        var split = viewCountElement.Split([' '], StringSplitOptions.TrimEntries)
                            .Select(s => s.Replace(" ", ""));

                        var str = split
                            .Where(s => 
                                int.TryParse(s, out _))
                            .Aggregate("", (current, s) => current + s)
                            .Trim();

                        if (!int.TryParse(str, out var viewCount))
                        {
                            throw new ApplicationException("Не удалось получить количество просмотров");
                        }

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
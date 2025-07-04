using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;
using ActivityChecker.IO;
using Drv;
using Drv.ChrDrvSettings;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
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

                        var jObj = JObject.Parse(trim);
                        var viewCountElement = 
                            jObj.SelectToken("..videoPrimaryInfoRenderer.viewCount.videoViewCountRenderer.viewCount.simpleText") ?? 
                            jObj.SelectToken("..videoDescriptionHeaderRenderer.views.simpleText");

                        if (viewCountElement is null)
                            throw new ApplicationException($"Не удалось получить количество просмотров: {line}");
                        
                        var viewCountStr = viewCountElement.ToString();

                        var split = viewCountStr.Split([' '], StringSplitOptions.TrimEntries)
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
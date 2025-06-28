using System.Collections.Immutable;
using System.Text.Json;
using ActivityChecker.IO;
using Drv;
using Drv.ChrDrvSettings;
using Drv.Stealth.Clients.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using ParserExtension;
using Polly;
using Spectre.Console;

namespace ActivityChecker.Parsers;

public class InstagramParser(ChrDrvSettingsWithAutoDriver drvSettings, ILogger<InstagramParser> logger, IHostApplicationLifetime lifetime) :  ISiteParser
{
    public string Url => "https://www.instagram.com";
    private readonly HashSet<string> _skip = [];
    private const string BaseXpath = "//div[@style='width: 100%;']/div/div[contains(@style,'display: flex; flex-direction: column; padding-bottom: 0px; padding-top: 0px; position: relative;')]/div/div/div/a";
    
    public async Task<List<ViewResult>> GetViewCount(ImmutableList<string> lines)
    {
        var result = new List<ViewResult>();

        using var drv = await ChrDrvFactory.Create(drvSettings);

        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("Получение аккаунтов...", maxValue: lines.Count);
                
                foreach (var line in lines)
                {
                    try
                    {
                        var accountViewCount = new ViewResult()
                        {
                            Url = line,
                            Views = 0
                        };
                        
                        var currentUrl = line;
                        
                        if (!currentUrl.EndsWith("/reels") || !currentUrl.EndsWith("/reels/"))
                        {
                            currentUrl = $"{line}/reels/";
                        }
                        
                        await drv.Navigate().GoToUrlAsync(currentUrl);
                        drv.SpecialWait(1500);
                        
                        var videosContainer = await drv.GetElement(By.XPath(BaseXpath), 7);
                        if (videosContainer is not null)
                        {
                            foreach (var videoDiv in ScrollAndGetUrls(drv))
                            {
                                if (lifetime.ApplicationStopping.IsCancellationRequested) break;
                                if (videoDiv is null) break;

                                accountViewCount.Views += videoDiv.Views;

                                var xpath = $"//a[contains(@href,'{videoDiv.Url}')]/..";
                                
                                drv.FocusAndScrollToElement(xpath);
                                drv.HighlightElementByXPath(xpath, "yellow");
                            }
                        }
                        else
                        {
                            logger.LogError("Видео не обнаружены: {Url}", line);
                        }

                        result.Add(accountViewCount);
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

    private IEnumerable<ViewResult?> ScrollAndGetUrls(ChrDrv drv)
    {
        var getPolicy = Policy
            .HandleResult<ViewResult?>(result => result == null)
            .WaitAndRetry(2, _ => TimeSpan.FromSeconds(3));
        
        while (!lifetime.ApplicationStopping.IsCancellationRequested)
        {
            var div = getPolicy.Execute(() => GetVideoUrls(drv));
            yield return div;
        }
    }

    private ViewResult? GetVideoUrls(ChrDrv drv)
    {
        var parse = drv.PageSource.GetParse();
        if (parse is null) return null;
        
        
        var xpathCollection = parse.GetXPaths(BaseXpath);
        if (xpathCollection.Count == 0) return null;

        foreach (var videoXpath in xpathCollection)
        {
            var url = parse.GetAttributeValue(videoXpath);
            if (string.IsNullOrEmpty(url)) continue;
            if (!_skip.Add(url)) continue;

            var viewCountStr = parse.GetInnerText($"{videoXpath}/div[2]/div[2]//span[@dir='auto' and @style='----base-line-clamp-line-height: 20px; --lineHeight: 20px;']/span");
            if (string.IsNullOrEmpty(viewCountStr)) continue;
            if (!int.TryParse(viewCountStr, out var viewCount)) continue;

            return new ViewResult
            {
                Url = url,
                Views = viewCount
            };
        }
        
        return null;
    }
}
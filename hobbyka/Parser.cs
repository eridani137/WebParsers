using Drv;
using Drv.Stealth.Clients.Extensions;
using Flurl.Http;
using ParserExtension;
using Serilog;
using Shared;
using Spectre.Console;

namespace hobbyka;

public class Parser(ChrDrv drv)
{
    private const string Root = "//div[@class='element_description']";

    public async Task ProcessUrl(string url)
    {
        var splitUrl = url.Replace(Program.SiteUrl, "");
        var nodeXpath = $"//div[@id='catalog_list_of_elements']//a[@class='product-link' and @href='{splitUrl}']";
        drv.FocusAndScrollToElement(nodeXpath);
        drv.HighlightElementByXPath(nodeXpath);

        await drv.Navigate().GoToUrlAsync(url);
        var parse = drv.PageSource.GetParse();
        if (parse is null)
        {
            Log.Error("parse is null. {Url}", url);
            return;
        }

        var name = parse.GetInnerText($"{Root}//h1");
        var artStr = parse.GetAttributeValue($"{Root}//div[@class='element_article']/meta", "content");
        var art = int.Parse(artStr ?? string.Empty);
        var priceStr = parse.GetInnerText($"{Root}//span[@id='catalog_item_price_top']");
        var price = decimal.Parse(priceStr ?? string.Empty);
        var characteristicsList =
            parse.GetInnerTextValues(
                $"{Root}//div[@class='element_comp elem_specs elem_desc']");
        var characteristics = string.Join(Environment.NewLine, characteristicsList);

        if (name is null || art == 0 || price == 0 || characteristics == string.Empty)
        {
            Log.Error("Пустые значения. {Url}", url);
        }

        AnsiConsole.MarkupLine("Начинаю загрузку картинок...".MarkupSecondary());

        var imageUrls = parse.GetAttributeValues("//div[@id='toggle_photo']/div/ul/li//img", "src")
            .Select(u => $"{Program.SiteUrl}{u}").ToList();
        AnsiConsole.MarkupLine($"Найдено {imageUrls.Count} картинок".MarkupSecondary());

        var currentImagesPath = Path.Join(Program.BaseImagesPath, artStr);
        Directory.CreateDirectory(currentImagesPath);

        var cookies = drv.GetCookiesAsString();
        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("Загружаю картинки...".MarkupPrimary(), true, imageUrls.Count);

                foreach (var (i, imageUrl) in imageUrls.Index())
                {
                    try
                    {
                        var ext = Path.GetExtension(imageUrl);
                        await imageUrl
                            .WithCookies(cookies)
                            .DownloadFileAsync(currentImagesPath, $"{i}.{ext}");
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Ошибка в цикле загрузки картинок");
                    }
                    finally
                    {
                        task.Increment(1);
                    }
                }
            });

        AnsiConsole.MarkupLine("Все картинки загружены".MarkupSecondary());

        // var filter = Builders<ElementEntity>.Filter.Eq(e => e.Url, url);
        //
        // var entity = new ElementEntity
        // {
        //     Url = url,
        //     Art = art,
        //     Name = name!,
        //     Price = price
        // };
        //
        // await collection.ReplaceOneAsync(
        //     filter,
        //     entity,
        //     new ReplaceOptions { IsUpsert = true }); // TODO

        AnsiConsole.MarkupLine("Завершение обработки...".MarkupSecondary());
        drv.SpecialWait(2000);
        await drv.Navigate().BackAsync();
    }
}
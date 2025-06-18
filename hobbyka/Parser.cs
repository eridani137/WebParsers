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

    public async Task<ElementEntity?> ProcessUrl(string url)
    {
        await drv.Navigate().GoToUrlAsync(url);
        var parse = drv.PageSource.GetParse();
        if (parse is null)
        {
            Log.Error("parse is null");
            return null;
        }

        var name = parse.GetInnerText($"{Root}//h1");
        var artStr = parse.GetAttributeValue($"{Root}//div[@class='element_article']/meta", "content");
        var art = int.Parse(artStr ?? string.Empty);
        var characteristicsList =
            parse.GetInnerTextValues(
                $"{Root}//div[@class='element_comp elem_specs elem_desc']");
        var characteristics = string.Join(Environment.NewLine, characteristicsList);
        var colorsList =
            parse.GetInnerTextValues(
                $"{Root}//div[@class='element_comp elem_colors']/ul/li/label//span[@class='fl_span']");
        var tagsList = parse.GetInnerTextValues($"{Root}//div[@class='element_comp elem_tags']/a");
        var variantXpaths = parse.GetXPaths($"{Root}//div[@class='element_comp elem_size']/ul/li");
        var variants = new List<ElementVariant>();
        foreach (var variantXpath in variantXpaths)
        {
            try
            {
                var priceStr = parse.GetAttributeValue($"{variantXpath}/meta[@itemprop='price']", "content")
                    ?? parse.GetAttributeValue($"{variantXpath}/input", "data-price");
                var price = decimal.Parse(priceStr ?? string.Empty);
                var label = parse.GetInnerText($"{variantXpath}/label[@for]");
                if (label is null) throw new Exception("label is null");
                variants.Add(new ElementVariant()
                {
                    Label = label,
                    Price = price
                });
            }
            catch (Exception e)
            {
                Log.Error(e, "Ошибка при обработке вариантов");
            }
;       }

        if (name is null || art == 0 || characteristics == string.Empty || tagsList.Count == 0 || variants.Count == 0)
        {
            Log.Error("Пустые значения");
            return null;
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

        var entity = new ElementEntity
        {
            Url = url,
            Art = art,
            Name = name,
            Characteristics = characteristics,
            Colors = colorsList,
            Tags = tagsList,
            Variants = variants
        };

        AnsiConsole.MarkupLine("Завершение обработки...".MarkupSecondary());

        return entity;
    }
}
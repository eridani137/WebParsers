using Drv;
using Drv.ChrDrvSettings;
using Drv.Stealth.Clients.Extensions;
using Flurl.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ParserExtension;
using Shared;
using Spectre.Console;

namespace hobbyka;

public class Parser(
    IMongoClient client,
    ChrDrvSettingsWithAutoDriver drvSettings,
    IOptions<AppSettings> appSettings,
    ILogger<Parser> logger)
    : BackgroundService
{
    private const string SiteUrl = "https://hobbyka.ru";
    private const string BaseImagesPath = "images";
    private const string RootXpath = "//div[@class='element_description']";
    private ChrDrv _drv = null!;
    private IMongoDatabase _database = null!;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _database = client.GetDatabase("hobbyka");
        Directory.CreateDirectory(BaseImagesPath);
        _drv = await ChrDrvFactory.Create(drvSettings);

        if (appSettings.Value.SingleUrls.Count > 0)
        {
            var grid = new Grid();
            grid.AddColumn(new GridColumn());
            grid.AddRow(new Markup($"Режим отдельных ссылок".MarkupPrimary()));
            var panel = new Panel(grid)
                .BorderColor(Color.Yellow)
                .Border(SpectreConfig.BoxBorder);
            panel.Width = AnsiConsole.Profile.Width;
            AnsiConsole.Write(panel);

            foreach (var (categoryName, urls) in appSettings.Value.SingleUrls)
            {
                foreach (var url in urls)
                {
                    await ProcessEntity(url, categoryName);
                }
            }
        }
        else
        {
            foreach (var (categoryName, categoryUrl) in appSettings.Value.DownloadCategories)
            {
                var grid = new Grid();
                grid.AddColumn(new GridColumn());
                grid.AddRow(new Markup($"Текущая категория: {categoryName}".MarkupPrimary()));
                grid.AddRow(new Markup($"Текущий URL: {categoryUrl}".MarkupPrimary()));
                var panel = new Panel(grid)
                    .BorderColor(Color.Yellow)
                    .Border(SpectreConfig.BoxBorder);
                panel.Width = AnsiConsole.Profile.Width;
                AnsiConsole.Write(panel);

                await _drv.Navigate().GoToUrlAsync(categoryUrl);

                _drv.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
                _drv.SpecialWait(2000);
                _drv.ExecuteScript("window.scrollTo(0, 0)");
                _drv.SpecialWait(2000);
                _drv.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
                _drv.SpecialWait(2000);
                _drv.ExecuteScript("window.scrollTo(0, 0)");
                _drv.SpecialWait(2000);
                _drv.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");

                var parse = _drv.PageSource.GetParse();
                if (parse is null) throw new Exception("root parse is null");
                var urls = parse.GetAttributeValues(
                        "//div[@id='catalog_list_of_elements']//div[@class='view_element']//a[@class='product-link']")
                    .Select(u => $"{SiteUrl}{u}").ToList();
                AnsiConsole.MarkupLine($"Прочитано {urls.Count} строк".MarkupSecondary());

                AnsiConsole.MarkupLine("Начинаю обработку...".MarkupSecondary());

                foreach (var url in urls)
                {
                    try
                    {
                        AnsiConsole.MarkupLine($"Обработка: {url}".MarkupSecondary());

                        var splitUrl = url.Replace(SiteUrl, "");
                        var nodeXpath =
                            $"//div[@id='catalog_list_of_elements']//a[@class='product-link' and @href='{splitUrl}']";
                        _drv.FocusAndScrollToElement(nodeXpath);
                        _drv.HighlightElementByXPath(nodeXpath);

                        await ProcessEntity(url, categoryName);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Ошибка в цикле обработки ссылок: {Url}", url);
                    }
                }
            }
        }

        _drv.Dispose();

        AnsiConsole.MarkupLine("Все категории обработаны".MarkupPrimary());
        AnsiConsole.MarkupLine("Нажмите любую клавишу для выхода...".MarkupPrimary());
        Console.ReadKey(true);
    }

    private async Task ProcessEntity(string url, string categoryName)
    {
        var collection = _database.GetCollection<ElementEntity>(categoryName);
        
        var entity = await ProcessUrl(url);
        if (entity is null) return;

        var filter = Builders<ElementEntity>.Filter.Eq(e => e.Url, url);
        await collection.ReplaceOneAsync(
            filter,
            entity,
            new ReplaceOptions { IsUpsert = true });

        _drv.SpecialWait(2000);
        await _drv.Navigate().BackAsync();
    }

    private async Task<ElementEntity?> ProcessUrl(string url)
    {
        await _drv.Navigate().GoToUrlAsync(url);
        var parse = _drv.PageSource.GetParse();
        if (parse is null)
        {
            logger.LogError("parse is null: {Url}", url);
            return null;
        }

        var name = parse.GetInnerText($"{RootXpath}//h1");
        var artStr = parse.GetAttributeValue($"{RootXpath}//div[@class='element_article']/meta", "content");
        var art = int.Parse(artStr ?? string.Empty);
        var characteristicsList =
            parse.GetInnerTextValues(
                $"{RootXpath}//div[@class='element_comp elem_specs elem_desc']");
        var characteristics = string.Join(Environment.NewLine, characteristicsList);
        var colorsList =
            parse.GetInnerTextValues(
                $"{RootXpath}//div[@class='element_comp elem_colors']/ul/li/label//span[@class='fl_span']");
        var tagsList = parse.GetInnerTextValues($"{RootXpath}//div[@class='element_comp elem_tags']/a");
        var variantXpaths = parse.GetXPaths($"{RootXpath}//div[@class='element_comp elem_size']/ul/li");
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
                logger.LogError(e, "Ошибка при обработке вариантов: {Url}", url);
            }
        }

        if (name is null || art == 0 || characteristics == string.Empty || variants.Count == 0)
        {
            logger.LogError("Пустые значения: {Url}", url);
            return null;
        }

        AnsiConsole.MarkupLine("Начинаю загрузку картинок...".MarkupSecondary());

        var imageUrls = parse.GetAttributeValues("//div[@id='toggle_photo']/div/ul/li//img", "src")
            .Select(u => $"{SiteUrl}{u}").ToList();
        AnsiConsole.MarkupLine($"Найдено {imageUrls.Count} картинок".MarkupSecondary());

        var currentImagesPath = Path.Join(BaseImagesPath, artStr);
        Directory.CreateDirectory(currentImagesPath);

        var cookies = _drv.GetCookiesAsString();
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
                        logger.LogError(e, "Ошибка в цикле загрузки картинок");
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
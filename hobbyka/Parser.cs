using Drv;
using Drv.ChrDrvSettings;
using Drv.Stealth.Clients.Extensions;
using Flurl.Http;
using hobbyka.Entity;
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
    IHostApplicationLifetime lifetime,
    ILogger<Parser> logger)
{
    private const string SiteUrl = "https://hobbyka.ru";
    private const string BaseImagesPath = "images";
    private const string RootXpath = "//div[@class='element_description']";
    private ChrDrv _drv = null!;
    private IMongoDatabase _database = null!;

    public async Task ExecuteAsync()
    {
        _database = client.GetDatabase("hobbyka");
        Directory.CreateDirectory(BaseImagesPath);
        _drv = await ChrDrvFactory.Create(drvSettings);

        if (appSettings.Value.SingleUrls.Count > 0)
        {
            var grid = new Grid();
            grid.AddColumn(new GridColumn());
            grid.AddRow(new Markup($"Режим отдельных ссылок".MarkupAqua()));
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
                grid.AddRow(new Markup($"Текущая категория: {categoryName}".MarkupAqua()));
                grid.AddRow(new Markup($"Текущий URL: {categoryUrl}".MarkupAqua()));
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
                AnsiConsole.MarkupLine($"Прочитано {urls.Count} строк".MarkupYellow());

                AnsiConsole.MarkupLine("Начинаю обработку...".MarkupYellow());

                foreach (var url in urls)
                {
                    try
                    {
                        AnsiConsole.MarkupLine($"Обработка: {url}".MarkupYellow());

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

        AnsiConsole.MarkupLine("Все категории обработаны".MarkupAqua());
        AnsiConsole.MarkupLine("Завершение...".MarkupAqua());
        
        lifetime.StopApplication();
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
        var shortDescription = string.Empty;
        var shortDescriptionNode = parse.GetNodeByXPath($"{RootXpath}//div[@class='element_comp elem_specs elem_desc']/div[@class='h3' and contains(text(), 'Технические характеристики')]/..");
        if (shortDescriptionNode is not null)
        {
            var divNode =
                parse.GetNodeByXPath(
                    $"{RootXpath}//div[@class='element_comp elem_specs elem_desc']/div[@class='h3' and contains(text(), 'Технические характеристики')]");
            if (divNode is not null)
            {
                shortDescriptionNode.RemoveChild(divNode);
            }
            
            shortDescription = shortDescriptionNode.InnerText.DecodeHtmlAndTrim();
        }
        var descriptionList =
            parse.GetInnerTextValues(
                $"{RootXpath}//div[@class='element_comp elem_specs elem_desc']")
                .Where(s => !s.StartsWith("Производитель оставляет за собой право изменять конфигурацию"));
        var description = string.Join(Environment.NewLine, descriptionList.Select(s => $"<div>{s}</div>"));
        var colorsList =
            parse.GetInnerTextValues(
                    $"{RootXpath}//div[@class='element_comp elem_colors']/ul/li/label//span[@class='fl_span']")
                .Select(color => char.ToUpper(color[0]) + color[1..])
                .ToList();
        var tagsList = parse.GetInnerTextValues($"{RootXpath}//div[@class='element_comp elem_tags']/a");
        var breadcrumbList =
            parse.GetInnerTextValues(
                $"{RootXpath}//div[@class='element_comp elem_breadcrumps']/div/a[@title!='Главная' and @title!='Каталог']");
        var breadcrumb = string.Join(" > ", breadcrumbList);
        var variantXpaths = parse.GetXPaths($"{RootXpath}//div[@class='element_comp elem_size']/ul/li");
        var variants = new List<ElementVariant>();
        foreach (var variantXpath in variantXpaths)
        {
            try
            {
                var idStr = parse.GetAttributeValue($"{variantXpath}/input", "value");
                var id = int.Parse(idStr ?? string.Empty);
                var priceStr = parse.GetAttributeValue($"{variantXpath}/meta[@itemprop='price']", "content")
                               ?? parse.GetAttributeValue($"{variantXpath}/input", "data-price");
                var price = decimal.Parse(priceStr ?? string.Empty);
                var label = parse.GetInnerText($"{variantXpath}/label[@for]");
                if (label is null) throw new Exception("label is null");
                variants.Add(new ElementVariant()
                {
                    Id = id,
                    Label = label,
                    Price = price
                });
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка при обработке вариантов: {Url}", url);
            }
        }

        if (name is null || art == 0 || variants.Count == 0)
        {
            logger.LogError("Пустые значения: {Url}", url);
            return null;
        }

        AnsiConsole.MarkupLine("Начинаю загрузку картинок...".MarkupYellow());

        var imageUrls = parse.GetAttributeValues("//div[@id='toggle_photo']/div/ul/li//img", "src")
            .Select(u => $"{SiteUrl}{u}").ToList();
        AnsiConsole.MarkupLine($"Найдено {imageUrls.Count} картинок".MarkupYellow());

        var currentImagesPath = Path.Join(BaseImagesPath, artStr);
        Directory.CreateDirectory(currentImagesPath);

        var cookies = _drv.GetCookiesAsString();
        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("Загружаю картинки...".MarkupAqua(), true, imageUrls.Count);

                foreach (var (i, imageUrl) in imageUrls.Index())
                {
                    try
                    {
                        var ext = Path.GetExtension(imageUrl);
                        await imageUrl
                            .WithCookies(cookies)
                            .DownloadFileAsync(currentImagesPath, $"{i}{ext}");
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

        AnsiConsole.MarkupLine("Все картинки загружены".MarkupYellow());

        var entity = new ElementEntity
        {
            Url = url,
            Art = art,
            Name = name,
            ShortDescription = shortDescription,
            Description = description,
            Colors = colorsList,
            Tags = tagsList,
            Breadcrumb = breadcrumb,
            Variants = variants
        };

        AnsiConsole.MarkupLine("Завершение обработки...".MarkupYellow());

        return entity;
    }
}
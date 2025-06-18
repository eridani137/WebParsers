using Drv;
using Drv.Stealth.Clients.Extensions;
using Flurl.Http;
using hobbyka;
using MongoDB.Bson;
using MongoDB.Driver;
using ParserExtension;
using Serilog;
using Shared;
using Spectre.Console;

const string baseImagesPath = "images";
Directory.CreateDirectory(baseImagesPath);

const string siteUrl = "https://hobbyka.ru";
var categories = new Dictionary<string, string>()
{
    ["скамейки"] = $"{siteUrl}/catalog/skameyki/"
};
var (currentName, currentRootUrl) = categories.ElementAt(0);

var grid = new Grid();
grid.AddColumn(new GridColumn());
grid.AddRow(new Markup($"Текущая категория: {currentName}".MarkupPrimary()));
grid.AddRow(new Markup($"Текущий URL: {currentRootUrl}".MarkupPrimary()));
var panel = new Panel(grid)
    .BorderColor(Color.Yellow)
    .Border(SpectreConfig.BoxBorder);
panel.Width = AnsiConsole.Profile.Width;
AnsiConsole.Write(panel);

var client = new MongoClient("mongodb://eridani:qwerty@localhost:27017/");
var database = client.GetDatabase("hobbyka");
var collection = database.GetCollection<ElementEntity>(currentName);

var urls = await File.ReadAllLinesAsync($"{currentName}.txt");
AnsiConsole.MarkupLine($"Прочитано {urls.Length} строк".MarkupSecondary());

using var drv = await ChrDrvFactory.Create(Configuration.DrvSettings);
await drv.Navigate().GoToUrlAsync(currentRootUrl);

AnsiConsole.MarkupLine("Начинаю обработку...".MarkupSecondary());

const string root = "//div[@class='element_description']";
foreach (var url in urls)
{
    AnsiConsole.MarkupLine($"Обработка: {url}".MarkupSecondary());
    try
    {
        var splitUrl = url.Replace(siteUrl, "");
        var nodeXpath = $"//div[@id='catalog_list_of_elements']//a[@class='product-link' and @href='{splitUrl}']";
        drv.FocusAndScrollToElement(nodeXpath);
        drv.HighlightElementByXPath(nodeXpath);
        
        await drv.Navigate().GoToUrlAsync(url);
        var parse = drv.PageSource.GetParse();
        if (parse is null)
        {
            Log.Error("parse is null. {Url}", url);
            continue;
        }

        var name = parse.GetInnerText($"{root}//h1");
        var artStr = parse.GetAttributeValue($"{root}//div[@class='element_article']/meta", "content");
        var art = int.Parse(artStr ?? string.Empty);
        var priceStr = parse.GetInnerText($"{root}//span[@id='catalog_item_price_top']");
        var price = decimal.Parse(priceStr ?? string.Empty);

        if (name is null || art == 0 || price == 0)
        {
            Log.Error("Пустые значения. {Url}", url);
        }
        
        AnsiConsole.MarkupLine("Начинаю загрузку картинок...".MarkupSecondary());

        var imageUrls = parse.GetAttributeValues("//div[@id='toggle_photo']/div/ul/li//img", "src")
            .Select(u => $"{siteUrl}{u}").ToList();
        AnsiConsole.MarkupLine($"Найдено {imageUrls.Count} картинок".MarkupSecondary());

        var currentImagesPath = Path.Join(baseImagesPath, artStr);
        Directory.CreateDirectory(currentImagesPath);
        
        var cookies = drv.GetCookiesAsString();
        await AnsiConsole.Progress()
            .StartAsync(async ctx=>
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
    catch (Exception e)
    {
        Log.Error(e, "Ошибка в цикле обработки ссылок");
    }
}

AnsiConsole.MarkupLine("Нажмите любую клавишу для выхода...".MarkupSecondary());
Console.ReadKey(true);
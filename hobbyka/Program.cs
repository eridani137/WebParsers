using Drv;
using hobbyka;
using MongoDB.Bson;
using MongoDB.Driver;
using ParserExtension;
using Serilog;
using Shared;
using Spectre.Console;

var categories = new Dictionary<string, string>()
{
    ["скамейки"] = "https://hobbyka.ru/catalog/skameyki/"
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

const string root = "//div[@class='element_description']";

foreach (var url in urls)
{
    try
    {
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

        var filter = Builders<ElementEntity>.Filter.Eq(e => e.Url, url);

        var entity = new ElementEntity
        {
            Url = url,
            Art = art,
            Name = name!,
            Price = price
        };

        await collection.ReplaceOneAsync(
            filter,
            entity,
            new ReplaceOptions { IsUpsert = true });
    }
    catch (Exception e)
    {
        Log.Error(e, "Ошибка в цикле обработки ссылок");
    }
}

AnsiConsole.MarkupLine("Нажмите любую клавишу для выхода...".MarkupSecondary());
Console.ReadKey(true);
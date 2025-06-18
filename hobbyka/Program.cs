using Drv;
using Drv.Stealth.Clients.Extensions;
using MongoDB.Driver;
using Serilog;
using Shared;
using Spectre.Console;

namespace hobbyka;

internal static class Program
{
    public const string BaseImagesPath = "images";
    public const string SiteUrl = "https://hobbyka.ru";
    
    public static async Task Main()
    {
        Directory.CreateDirectory(BaseImagesPath);
        
        var categories = new Dictionary<string, string>()
        {
            ["скамейки"] = $"{SiteUrl}/catalog/skameyki/"
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
        var parser = new Parser(drv);
        foreach (var url in urls)
        {
            try
            {
                AnsiConsole.MarkupLine($"Обработка: {url}".MarkupSecondary());
                
                var splitUrl = url.Replace(SiteUrl, "");
                var nodeXpath = $"//div[@id='catalog_list_of_elements']//a[@class='product-link' and @href='{splitUrl}']";
                drv.FocusAndScrollToElement(nodeXpath);
                drv.HighlightElementByXPath(nodeXpath);
                
                var entity = await parser.ProcessUrl(url);
                if (entity is null) continue;
                
                // var filter = Builders<ElementEntity>.Filter.Eq(e => e.Url, url);
                // await collection.ReplaceOneAsync(
                //     filter,
                //     entity,
                //     new ReplaceOptions { IsUpsert = true }); // TODO
                
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
    }
}
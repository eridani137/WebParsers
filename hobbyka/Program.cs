using Drv;
using Drv.Stealth.Clients.Extensions;
using MongoDB.Driver;
using ParserExtension;
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
            ["Скамейки"] = $"{SiteUrl}/catalog/skameyki/",
            ["Урны"] = $"{SiteUrl}/catalog/urny/",
            ["Перголы"] = $"{SiteUrl}/catalog/pergoly/"
        };

        foreach (var (categoryName, categoryUrl) in categories)
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

            var client = new MongoClient("mongodb://eridani:qwerty@localhost:27017/");
            var database = client.GetDatabase("hobbyka");
            var collection = database.GetCollection<ElementEntity>(categoryName);

            using var drv = await ChrDrvFactory.Create(Configuration.DrvSettings);
            await drv.Navigate().GoToUrlAsync(categoryUrl);
            
            drv.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
            drv.SpecialWait(2000);
            drv.ExecuteScript("window.scrollTo(0, 0)");
            drv.SpecialWait(2000);
            drv.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
            drv.SpecialWait(2000);
            drv.ExecuteScript("window.scrollTo(0, 0)");
            drv.SpecialWait(2000);
            drv.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
            
            var parser = new Parser(drv);
            var parse = drv.PageSource.GetParse();
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
                    drv.FocusAndScrollToElement(nodeXpath);
                    drv.HighlightElementByXPath(nodeXpath);

                    var entity = await parser.ProcessUrl(url);
                    if (entity is null) continue;

                    var filter = Builders<ElementEntity>.Filter.Eq(e => e.Url, url);
                    await collection.ReplaceOneAsync(
                        filter,
                        entity,
                        new ReplaceOptions { IsUpsert = true });

                    drv.SpecialWait(2000);
                    await drv.Navigate().BackAsync();
                }
                catch (Exception e)
                {
                    Log.Error(e, "Ошибка в цикле обработки ссылок");
                }
            }
        }

        AnsiConsole.MarkupLine("Все категории обработаны".MarkupPrimary());
        AnsiConsole.MarkupLine("Нажмите любую клавишу для выхода...".MarkupPrimary());
        Console.ReadKey(true);
    }
}
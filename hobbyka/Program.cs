using Drv;
using MongoDB.Bson;
using MongoDB.Driver;
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
var collection = database.GetCollection<BsonDocument>(currentName);

var urls = await File.ReadAllLinesAsync($"{currentName}.txt");
AnsiConsole.MarkupLine($"Прочитано {urls.Length} строк".MarkupSecondary());

using var drv = await ChrDrvFactory.Create(Configuration.DrvSettings);
drv.Navigate().GoToUrl(currentRootUrl);


AnsiConsole.MarkupLine("Нажмите любую клавишу для выхода...".MarkupSecondary());
Console.ReadKey(true);
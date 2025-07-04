using hobbyka.WooCommerce;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shared;
using Spectre.Console;

namespace hobbyka;

public class Exporter(WooCommerceExporter exporter, IOptions<AppSettings> appSettings, IHostApplicationLifetime lifetime)
{
    public async Task ExecuteAsync()
    {
        const string exportPath = "export";
        Directory.CreateDirectory(exportPath);

        var excludeUrls = (await File.ReadAllLinesAsync("exclude.txt", lifetime.ApplicationStopping));
        
        var grid = new Grid();
        grid.AddColumn(new GridColumn());
        grid.AddRow(new Markup("Режим экспорта".MarkupAqua()));
        grid.AddRow(new Markup($"{string.Join(", ", appSettings.Value.Export)}".MarkupAqua()));
        grid.AddRow(new Markup($"Исключено ссылок: {excludeUrls.Length}".MarkupAqua()));
        var panel = new Panel(grid)
            .BorderColor(Color.Yellow)
            .Border(SpectreConfig.BoxBorder);
        panel.Width = AnsiConsole.Profile.Width;
        AnsiConsole.Write(panel);
        
        foreach (var currentCategory in appSettings.Value.Export)
        {
            var fileName = $"{currentCategory}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv";
            var filePath = Path.Join(exportPath, fileName);

            await exporter.ExportToCsvAsync(filePath, currentCategory, excludeUrls);
        }
        
        AnsiConsole.MarkupLine("Все категории обработаны".MarkupAqua());
        AnsiConsole.MarkupLine("Завершение...".MarkupAqua());
        
        lifetime.StopApplication();
    }
}
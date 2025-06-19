using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using MongoDB.Driver;

namespace hobbyka.WooCommerce;

public class WooCommerceExporter(IMongoClient client)
{
    public async Task ExportToCsvAsync(string filePath, string categoryName, string[] excludeUrls)
    {
        var database = client.GetDatabase("hobbyka");
        var collection = database.GetCollection<ElementEntity>(categoryName);
        var products = await collection.Find(_ => true).ToListAsync();

        var wooCommerceRecords = products
            .Where(p => !excludeUrls.Any(e => e.EndsWith(p.Url)))
            .Select(p => new WooCommerceRecord
            {
                Артикул = p.Art.ToString(),
                Имя = p.Name,
                Описание = p.Characteristics,
                Категории = string.Join(", ", p.Tags),
                Значения_атрибутов_1 = string.Join(", ", p.Colors)
            }).ToList();

        await using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
        await using var csv = new CsvWriter(writer,
            new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = true,
                Encoding = System.Text.Encoding.UTF8
            });
        csv.Context.RegisterClassMap<WooCommerceMap>();
        await csv.WriteRecordsAsync(wooCommerceRecords);
    }
}
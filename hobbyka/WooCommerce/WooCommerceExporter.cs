using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using hobbyka.Entity;
using MongoDB.Driver;

namespace hobbyka.WooCommerce;

public class WooCommerceExporter(IMongoClient client)
{
    public async Task ExportToCsvAsync(string filePath, string categoryName, string[] excludeUrls)
    {
        var database = client.GetDatabase("hobbyka");
        var collection = database.GetCollection<ElementEntity>(categoryName);
        var products = await collection.Find(_ => true).ToListAsync();

        var result = new List<WooCommerceRecord>();
        var index = 5000;
        foreach (var product in products.Where(entity => !excludeUrls.Any(s => s.EndsWith(entity.Url))))
        {
            result.Add(new WooCommerceRecord()
            {
                ID = index.ToString(),
                Тип = "variable",
                Артикул = product.Art.ToString(),
                Имя = product.Name,
                Краткое_описание = $"<div class=\"compare_listing\">{product.ShortDescription}</>",
                Описание = product.Description,
                Категории = product.Breadcrumb,
                Значения_атрибутов_2 = string.Join(", ", product.Colors)
            });
            
            foreach (var variant in product.Variants)
            {
                index++;
                result.Add(new WooCommerceRecord()
                {
                    ID = index.ToString(),
                    Тип = "variation",
                    Имя = product.Name,
                    Налоговый_класс = "parent",
                    Базовая_цена = variant.Price.ToString(CultureInfo.InvariantCulture),
                    Родительский = product.Art.ToString(),
                    Значения_атрибутов_1 = variant.Label,
                    Видимость_атрибута_1 = "",
                    Название_атрибута_2 = "pa_цвет-древесины",
                    Видимость_атрибута_2 = ""
                });
            }
            
            index++;
        }

        await using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
        await using var csv = new CsvWriter(writer,
            new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = true,
                Encoding = System.Text.Encoding.UTF8
            });
        csv.Context.RegisterClassMap<WooCommerceMap>();
        await csv.WriteRecordsAsync(result);
    }
}
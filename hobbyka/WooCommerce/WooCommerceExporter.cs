using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using MongoDB.Driver;
using Spectre.Console;

namespace hobbyka.WooCommerce;

public class WooCommerceExporter(IMongoClient client)
{
    public async Task ExportToCsvAsync(string filePath, string categoryName, string[] excludeUrls)
    {
        var database = client.GetDatabase("hobbyka");
        var collection = database.GetCollection<ElementEntity>(categoryName);
        var products = await collection.Find(_ => true).ToListAsync();
        var wooCommerceProducts = new List<WooCommerceProduct>();

        foreach (var product in products)
        {
            if (excludeUrls.Any(u => u.EndsWith(product.Url)))
            {
                AnsiConsole.MarkupLine("Пропущен URL");
                continue;
            }
            if (product.Variants is { Count: > 1 })
            {
                // Создаем родительский продукт для вариантов
                wooCommerceProducts.Add(CreateParentProduct(product));
                
                // Создаем варианты продукта
                foreach (var variant in product.Variants)
                {
                    wooCommerceProducts.Add(CreateVariantProduct(product, variant));
                }
            }
            else
            {
                // Простой продукт без вариантов
                var price = product.Variants.FirstOrDefault()?.Price ?? 0;
                wooCommerceProducts.Add(CreateSimpleProduct(product, price));
            }
        }

        // Записываем CSV с помощью CsvHelper
        await using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
        await using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Quote = '"',
            Delimiter = ",",
            ShouldQuote = args => args.Field.Contains(',') || args.Field.Contains('"') || args.Field.Contains('\n')
        });

        csv.Context.RegisterClassMap<WooCommerceProductMap>();
        await csv.WriteRecordsAsync(wooCommerceProducts);
    }

    private WooCommerceProduct CreateParentProduct(ElementEntity product)
    {
        var categories = string.Join(", ", product.Tags ?? []);
        var colors = string.Join(" | ", product.Colors ?? []);
        var sizes = string.Join(" | ", product.Variants.Select(v => v.Label) ?? []);

        return new WooCommerceProduct
        {
            ID = product.Art.ToString(),
            Type = "variable",
            SKU = product.Art.ToString(),
            Name = product.Name,
            Published = "1",
            IsFeatured = "0",
            VisibilityInCatalog = "visible",
            ShortDescription = "",
            Description = product.Characteristics,
            DateSalePriceStarts = "",
            DateSalePriceEnds = "",
            TaxStatus = "taxable",
            TaxClass = "",
            InStock = "1",
            Stock = "",
            LowStockAmount = "",
            BackordersAllowed = "no",
            SoldIndividually = "no",
            Weight = "",
            Length = ExtractDimension(product.Characteristics, "Длина"),
            Width = ExtractDimension(product.Characteristics, "Ширина"),
            Height = ExtractDimension(product.Characteristics, "Высота"),
            AllowCustomerReviews = "1",
            PurchaseNote = "",
            SalePrice = "",
            RegularPrice = "",
            Categories = categories,
            Tags = categories,
            ShippingClass = "",
            Images = "",
            DownloadLimit = "",
            DownloadExpiryDays = "",
            Parent = "",
            GroupedProducts = "",
            Upsells = "",
            CrossSells = "",
            ExternalUrl = "",
            ButtonText = "",
            Position = "0",
            Attribute1Name = "Размер",
            Attribute1Values = sizes,
            Attribute1Visible = "1",
            Attribute1Global = "0",
            Attribute2Name = !string.IsNullOrEmpty(colors) ? "Цвет" : "",
            Attribute2Values = colors,
            Attribute2Visible = !string.IsNullOrEmpty(colors) ? "1" : "",
            Attribute2Global = !string.IsNullOrEmpty(colors) ? "0" : ""
        };
    }

    private WooCommerceProduct CreateVariantProduct(ElementEntity product, ElementVariant variant)
    {
        return new WooCommerceProduct
        {
            ID = $"{product.Art}-{GetVariantSku(variant)}",
            Type = "variation",
            SKU = $"{product.Art}-{GetVariantSku(variant)}",
            Name = $"{product.Name} - {variant.Label}",
            Published = "1",
            IsFeatured = "0",
            VisibilityInCatalog = "visible",
            ShortDescription = "",
            Description = "",
            DateSalePriceStarts = "",
            DateSalePriceEnds = "",
            TaxStatus = "taxable",
            TaxClass = "",
            InStock = "1",
            Stock = "10",
            LowStockAmount = "",
            BackordersAllowed = "no",
            SoldIndividually = "no",
            Weight = "",
            Length = "",
            Width = "",
            Height = "",
            AllowCustomerReviews = "1",
            PurchaseNote = "",
            SalePrice = "",
            RegularPrice = variant.Price.ToString("F2", CultureInfo.InvariantCulture),
            Categories = "",
            Tags = "",
            ShippingClass = "",
            Images = "",
            DownloadLimit = "",
            DownloadExpiryDays = "",
            Parent = product.Art.ToString(),
            GroupedProducts = "",
            Upsells = "",
            CrossSells = "",
            ExternalUrl = "",
            ButtonText = "",
            Position = "0",
            Attribute1Name = "Размер",
            Attribute1Values = variant.Label,
            Attribute1Visible = "1",
            Attribute1Global = "0",
            Attribute2Name = "",
            Attribute2Values = "",
            Attribute2Visible = "",
            Attribute2Global = ""
        };
    }

    private WooCommerceProduct CreateSimpleProduct(ElementEntity product, decimal price)
    {
        var categories = string.Join(", ", product.Tags ?? []);
        var colors = string.Join(" | ", product.Colors ?? []);

        return new WooCommerceProduct
        {
            ID = product.Art.ToString(),
            Type = "simple",
            SKU = product.Art.ToString(),
            Name = product.Name,
            Published = "1",
            IsFeatured = "0",
            VisibilityInCatalog = "visible",
            ShortDescription = "",
            Description = product.Characteristics,
            DateSalePriceStarts = "",
            DateSalePriceEnds = "",
            TaxStatus = "taxable",
            TaxClass = "",
            InStock = "1",
            Stock = "10",
            LowStockAmount = "",
            BackordersAllowed = "no",
            SoldIndividually = "no",
            Weight = ExtractWeight(product.Characteristics),
            Length = ExtractDimension(product.Characteristics, "Длина"),
            Width = ExtractDimension(product.Characteristics, "Ширина"),
            Height = ExtractDimension(product.Characteristics, "Высота"),
            AllowCustomerReviews = "1",
            PurchaseNote = "",
            SalePrice = "",
            RegularPrice = price > 0 ? price.ToString("F2", CultureInfo.InvariantCulture) : "",
            Categories = categories,
            Tags = categories,
            ShippingClass = "",
            Images = "",
            DownloadLimit = "",
            DownloadExpiryDays = "",
            Parent = "",
            GroupedProducts = "",
            Upsells = "",
            CrossSells = "",
            ExternalUrl = "",
            ButtonText = "",
            Position = "0",
            Attribute1Name = !string.IsNullOrEmpty(colors) ? "Цвет" : "",
            Attribute1Values = colors,
            Attribute1Visible = !string.IsNullOrEmpty(colors) ? "1" : "",
            Attribute1Global = !string.IsNullOrEmpty(colors) ? "0" : "",
            Attribute2Name = "",
            Attribute2Values = "",
            Attribute2Visible = "",
            Attribute2Global = ""
        };
    }

    private string GetVariantSku(ElementVariant variant)
    {
        return variant.Label.Replace(" ", "").Replace("(", "").Replace(")", "")
                           .Replace(",", "").Replace("м", "m");
    }

    private string ExtractDimension(string characteristics, string dimensionName)
    {
        if (string.IsNullOrEmpty(characteristics))
            return "";

        var lines = characteristics.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (!line.Contains(dimensionName) || !line.Contains("мм")) continue;
            var parts = line.Split(':');
            if (parts.Length > 1)
            {
                var value = parts[1].Trim().Replace("мм", "").Replace(" ", "");
                if (decimal.TryParse(value.Split('-')[0], out decimal mm))
                {
                    return (mm / 10).ToString("F1", CultureInfo.InvariantCulture); // Конвертируем в см
                }
            }
        }
        return "";
    }

    private string ExtractWeight(string characteristics)
    {
        if (string.IsNullOrEmpty(characteristics))
            return "";

        var lines = characteristics.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (line.Contains("Вес") && line.Contains("кг"))
            {
                var parts = line.Split(':');
                if (parts.Length > 1)
                {
                    var value = parts[1].Trim().Replace("кг", "").Replace("от", "").Trim();
                    if (decimal.TryParse(value.Split(' ')[0].Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out decimal weight))
                    {
                        return weight.ToString("F2", CultureInfo.InvariantCulture);
                    }
                }
            }
        }
        return "";
    }
}
using CsvHelper.Configuration;

namespace hobbyka.WooCommerce;

public sealed class WooCommerceCsvMap : ClassMap<WooCommerceCsvRecord>
{
    public WooCommerceCsvMap()
    {
        Map(m => m.ID).Name("ID");
        Map(m => m.Тип).Name("Тип");
        Map(m => m.Артикул).Name("Артикул");
        Map(m => m.GTIN_UPC_EAN_ISBN).Name("GTIN, UPC, EAN или ISBN");
        Map(m => m.Имя).Name("Имя");
        Map(m => m.Опубликован).Name("Опубликован");
        Map(m => m.Рекомендуемый).Name("Рекомендуемый?");
        Map(m => m.Видимость_в_каталоге).Name("Видимость в каталоге");
        Map(m => m.Краткое_описание).Name("Краткое описание");
        Map(m => m.Описание).Name("Описание");
        // ... остальные поля в том же порядке, как в CSV ...
        Map(m => m.Атрибут_1_по_умолчанию).Name("Атрибут 1 по умолчанию");
    }
}
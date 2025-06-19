using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace hobbyka.WooCommerce;

public class BooleanToIntConverter : DefaultTypeConverter
{
    public override string ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
    {
        if (value is bool boolValue)
        {
            return boolValue ? "1" : "0";
        }
        return "0";
    }

    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        return text == "1" || text?.ToLower() == "true";
    }
}
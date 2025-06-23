using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace ActivityChecker.IO;

public class CsvExporter
{
    public async Task Export(string filePath, List<ViewResult> results)
    {
        await using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
        await using var csv = new CsvWriter(writer,
            new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = true,
                Encoding = System.Text.Encoding.UTF8
            });
        csv.Context.RegisterClassMap<ViewResultMap>();
        await csv.WriteRecordsAsync(results);
        
        await csv.NextRecordAsync();
        csv.WriteField("ИТОГО:");
        csv.WriteField(results.Sum(r => r.Views));
        await csv.NextRecordAsync();
    }
}
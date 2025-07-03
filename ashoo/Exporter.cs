using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ashoo;

public class Exporter
{
    private IMongoCollection<ModelEntity> Models { get; }
    
    public Exporter(IMongoClient client)
    {
        var db = client.GetDatabase("ashoo");
        Models = db.GetCollection<ModelEntity>("models");
    }
    
    public async Task Export()
    {
        var models = await Models.Find(f => f.phone != null).ToListAsync() ?? [];

        foreach (var model in models)
        {
            model.phone ??= "-";
            model.phone = model.phone.Replace("tel:", "");
            model.telegram ??= "-";
            model.country ??= "Россия";
        }
        
        var fileName = $"export_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv";
        var filePath = Path.Join(fileName);
        
        await using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
        await using var csv = new CsvWriter(writer,
            new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = " | ",
                HasHeaderRecord = true,
                Encoding = System.Text.Encoding.UTF8
            });
        
        csv.Context.RegisterClassMap<ExportMap>();
        await csv.WriteRecordsAsync(models);
    }
}

public sealed class ExportMap : ClassMap<ModelEntity>
{
    public ExportMap()
    {
        Map(x => x.url).Name("url");
        Map(x => x.country).Name("country");
        Map(x => x.city).Name("city");
        Map(x => x.phone).Name("phone");
        Map(x => x.telegram).Name("telegram");
    }
}

public class ModelEntity
{
    public ObjectId _id { get; set; }
    public string url { get; set; }
    public string? country { get; set; }
    public string? city { get; set; }
    public string? phone { get; set; }
    public string? telegram { get; set; }
}
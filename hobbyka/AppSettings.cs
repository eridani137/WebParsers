namespace hobbyka;

public class AppSettings
{
    public Dictionary<string, string> DownloadCategories { get; } = [];
    public Dictionary<string, List<string>> SingleUrls { get; } = [];
    public List<string> Export { get; set; } = [];
}
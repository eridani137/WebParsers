using Drv;
using Drv.ChrDrvSettings;
using MongoDB.Driver;
using ParserExtension;
using Shared;
using Spectre.Console;

namespace escortnews;

public class Exporter
{
    private const string SiteUrl = "https://www.escortnews.com";
    private readonly ChrDrvSettingsWithAutoDriver _drvSettings;
    public IMongoCollection<Country> Countries { get; }
    
    public Exporter(IMongoClient client, ChrDrvSettingsWithAutoDriver drvSettings)
    {
        _drvSettings = drvSettings;
        var db = client.GetDatabase("escortnews");
        Countries = db.GetCollection<Country>("countries");
    }

    private async Task<Country?> GetCurrentLocation(ChrDrv drv)
    {
        // await drv.Navigate().GoToUrlAsync(SiteUrl);
        await drv.Navigate().GoToUrlAsync("https://www.scrapingcourse.com/cloudflare-challenge");
        
        var parse = drv.PageSource.GetParse();
        if (parse is null) throw new ApplicationException("parse is null");
        
        var countriesXpaths = parse.GetXPaths("//ul[@class='cityList']/li/span/a");
        foreach (var countryXpath in countriesXpaths)
        {
            var name = parse.GetInnerText(countryXpath);
            var url = parse.GetAttributeValue(countryXpath);
            
            if (name is null || url is null) throw new ApplicationException("url is null");

            if (Countries.Find(c => c.Name == name) is null)
            {
                return new Country()
                {
                    Name = name,
                    Url = url
                };
            }
        }

        return null;
    }

    public async Task Export()
    {
    }
}
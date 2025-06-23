using CsvHelper.Configuration;

namespace ActivityChecker.IO;

public sealed class ViewResultMap : ClassMap<ViewResult>
{
    public ViewResultMap()
    {
        Map(m => m.Url).Name("Url");
        Map(m => m.Views).Name("Views");
    }
}
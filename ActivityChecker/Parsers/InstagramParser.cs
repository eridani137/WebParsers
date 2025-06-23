using System.Collections.Immutable;
using ActivityChecker.IO;

namespace ActivityChecker.Parsers;

public class InstagramParser :  ISiteParser
{
    public string Url => "https://www.instagram.com";
    public Task<List<ViewResult>> GetViewCount(ImmutableList<string> lines)
    {
        throw new NotImplementedException();
    }
}
using System.Collections.Immutable;
using ActivityChecker.IO;

namespace ActivityChecker.Parsers;

public class YoutubeParser : ISiteParser
{
    public string Url => "https://youtube.com";
    public Task<List<ViewResult>> GetViewCount(ImmutableList<string> lines)
    {
        throw new NotImplementedException();
    }
}
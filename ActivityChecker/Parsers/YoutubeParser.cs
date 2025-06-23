namespace ActivityChecker.Parsers;

public class YoutubeParser : ISiteParser
{
    public string Url => "https://youtube.com";

    public int GetViewCount()
    {
        throw new NotImplementedException();
    }
}
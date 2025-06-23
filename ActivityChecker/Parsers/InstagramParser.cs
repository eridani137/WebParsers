namespace ActivityChecker.Parsers;

public class InstagramParser :  ISiteParser
{
    public string Url => "https://www.instagram.com";
    public int GetViewCount(string[] lines)
    {
        throw new NotImplementedException();
    }
}
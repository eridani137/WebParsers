namespace ActivityChecker.Parsers;

public class InstagramParser :  ISiteParser
{
    public string Url => "https://www.instagram.com";

    public int GetViewCount()
    {
        throw new NotImplementedException();
    }
}
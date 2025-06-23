namespace ActivityChecker.Parsers;

public class VkParser : ISiteParser
{
    public string Url => "https://vk.com";

    public int GetViewCount()
    {
        throw new NotImplementedException();
    }
}
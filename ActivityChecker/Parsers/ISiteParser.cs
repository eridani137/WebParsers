namespace ActivityChecker.Parsers;

public interface ISiteParser
{
    public string Url { get; }
    public int GetViewCount();
}
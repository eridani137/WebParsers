using Shared;

namespace ActivityChecker.Parsers;

public class VkParser : ISiteParser
{
    public string Url => "https://vk.com";

    public int GetViewCount(string[] lines)
    {
        var batches = lines.SplitIntoBatches(100);

        foreach (var batch in batches)
        {
            var postsQuery = string.Join(",", batch);
                
        }

        return 0;
    }
}
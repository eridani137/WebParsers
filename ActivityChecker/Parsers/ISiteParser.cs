using System.Collections.Immutable;
using ActivityChecker.IO;

namespace ActivityChecker.Parsers;

public interface ISiteParser
{
    public string Url { get; }
    public Task<List<ViewResult>> GetViewCount(ImmutableList<string> lines);
}
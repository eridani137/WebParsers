namespace ActivityChecker;

public class CheckerService
{
    public async Task CheckFile()
    {
        var strings = await File.ReadAllLinesAsync("");
    }
}
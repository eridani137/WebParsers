using ActivityChecker.Parsers;
using Drv;
using Drv.ChrDrvSettings;

namespace ActivityChecker;

public class CheckerService(ChrDrvSettingsWithoutDriver drvSettings, ParserFactory parserFactory)
{
    public async Task CheckFile()
    {
        var input = new PathUserInput();
        var strings = await File.ReadAllLinesAsync(input.Path.Trim('"'));
        
        var drv = await ChrDrvFactory.Create(drvSettings);

        foreach (var line in strings)
        {
            var parser = parserFactory.GetParser(line);
        }
    }
}
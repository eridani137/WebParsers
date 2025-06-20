using Microsoft.Extensions.Hosting;
using Serilog;
using Shared;
using Spectre.Console;

namespace hobbyka;

public class ConsoleMenu(IHostApplicationLifetime lifetime, Parser parser, Exporter exporter)
    : BaseConsoleMenu(lifetime)
{
    protected override async Task Worker()
    {
        const string parsing = "Парсинг";
        const string export = "Экспорт";

        while (!Lifetime.ApplicationStopping.IsCancellationRequested)
        {
            var choices = new SelectionPrompt<string>()
                .HighlightStyle(SpectreConfig.Style)
                .AddChoices(parsing, export);
            var prompt = AnsiConsole.Prompt(choices);

            try
            {
                switch (prompt)
                {
                    case parsing:
                        await parser.ExecuteAsync();
                        break;
                    case export:
                        await exporter.ExecuteAsync();
                        break;
                }
            }
            catch (Exception e)
            {
                Log.ForContext<ConsoleMenu>().Error(e, "Ошибка в цикле меню");
            }
        }
    }
}
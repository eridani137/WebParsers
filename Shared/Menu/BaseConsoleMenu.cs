using Microsoft.Extensions.Hosting;
using Serilog;
using Spectre.Console;

namespace Shared.Menu;

public abstract class BaseConsoleMenu(IHostApplicationLifetime lifetime) : IHostedService
{
    private Task? _task;

    protected abstract List<MenuItem> MenuItems { get; }

    protected string MenuTitle = "";

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _task = ShowMenu();
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_task != null)
            {
                await Task.WhenAny(_task, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }
        finally
        {
            _task?.Dispose();
            lifetime.StopApplication();
        }
    }

    private async Task ShowMenu()
    {
        MenuItems.Add(new MenuItem("Выход", () =>
        {
            lifetime.StopApplication();
            return Task.CompletedTask;
        }));
        
        while (!lifetime.ApplicationStopping.IsCancellationRequested)
        {
            await MenuHeader();
            var choice = new SelectionPrompt<MenuItem>()
                .Title(MenuTitle)
                .HighlightStyle(SpectreConfig.Style)
                .UseConverter(i => i.Title)
                .AddChoices(MenuItems);
            var prompt = AnsiConsole.Prompt(choice);
            try
            {
                await prompt.Task.Invoke();
            }
            catch (Exception e)
            {
                Log.ForContext<BaseConsoleMenu>().Error(e, "Ошибка при выполнении таски: {TaskTitle}", prompt.Title);
            }
        }
    }

    protected virtual Task MenuHeader()
    {
        return Task.CompletedTask;
    }
}
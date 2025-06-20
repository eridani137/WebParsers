using Microsoft.Extensions.Hosting;
using Serilog;
using Spectre.Console;

namespace Shared;

public abstract class BaseConsoleMenu(IHostApplicationLifetime lifetime) : IHostedService
{
    private Task? _task;
    protected abstract List<MenuItem> MenuItems { get; }

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
        while (!lifetime.ApplicationStopping.IsCancellationRequested)
        {
            await MenuHeader();
            var choice = new SelectionPrompt<MenuItem>()
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
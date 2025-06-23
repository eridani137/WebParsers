using Microsoft.Extensions.Hosting;
using Serilog;
using Spectre.Console;

namespace Shared.Menu;

public abstract class BaseConsoleMenu(IHostApplicationLifetime lifetime) : IHostedService
{
    private Task? _task;

    protected abstract List<BaseMenuItem> MenuItems { get; }

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
            var choice = new SelectionPrompt<BaseMenuItem>()
                .Title(MenuTitle)
                .HighlightStyle(SpectreConfig.Style)
                .UseConverter(i => i.Title)
                .AddChoices(MenuItems);
            var prompt = await AnsiConsole.PromptAsync(choice);
            try
            {
                switch (prompt)
                {
                    case MenuItem menuItem:
                        await menuItem.Task.Invoke();
                        break;
                    case SubMenuItem subMenuItem:
                        await ShowSubMenu(subMenuItem);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.ForContext<BaseConsoleMenu>().Error(e, "Ошибка при выполнении таски: {TaskTitle}", prompt.Title);
            }
        }
    }

    private static async Task ShowSubMenu(SubMenuItem subMenu)
    {
        var choice = new SelectionPrompt<MenuItem>()
            .Title(subMenu.Title)
            .HighlightStyle(SpectreConfig.Style)
            .UseConverter(i => i.Title)
            .AddChoices(subMenu.SubMenuItems);
        var prompt = await AnsiConsole.PromptAsync(choice);
        try
        {
            await prompt.Task.Invoke();
        }
        catch (Exception e)
        {
            Log.ForContext<BaseConsoleMenu>().Error(e, "Ошибка при выполнении таски: {TaskTitle}", subMenu.Title);
        }
    }

    protected virtual Task MenuHeader()
    {
        return Task.CompletedTask;
    }
}
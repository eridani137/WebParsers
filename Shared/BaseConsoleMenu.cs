using Microsoft.Extensions.Hosting;

namespace Shared;

public abstract class BaseConsoleMenu(IHostApplicationLifetime lifetime) : IHostedService
{
    private Task? _task;
    protected readonly IHostApplicationLifetime Lifetime = lifetime;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _task = Worker();
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
            Lifetime.StopApplication();
        }
    }
    
    protected abstract Task Worker();
}
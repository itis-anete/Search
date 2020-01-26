using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Search.Web.StartupHelpers
{
    public abstract class AsyncBackgroundService : BackgroundService
    {
        protected abstract Task RunAsync(CancellationToken stoppingToken);
        
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Factory.StartNew(
                () => RunAsync(stoppingToken),
                stoppingToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            return Task.CompletedTask;
        }
    }
}
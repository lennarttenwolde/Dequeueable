using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dequeueable.AmazonSQS.Services.Hosts
{
    internal sealed class QueueListenerHost(IHostExecutor hostExecutor, IHostApplicationLifetime hostApplicationLifetime, ILogger<QueueListenerHost> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await hostExecutor.HandleAsync(stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    hostApplicationLifetime.StopApplication();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unhandled exception occurred, shutting down the host");
                    hostApplicationLifetime.StopApplication();
                }
            }
        }
    }
}

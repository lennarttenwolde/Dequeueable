using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dequeueable.AzureQueueStorage.Services.Hosts
{
    internal sealed class JobHost(IHostExecutor hostHandler, IHostApplicationLifetime hostApplicationLifetime, ILogger<JobHost> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await hostHandler.HandleAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not TaskCanceledException)
            {
                logger.LogError(ex, "Unhandled exception occurred, shutting down the host");
                throw;
            }
            finally
            {
                if (!stoppingToken.IsCancellationRequested)
                {
                    hostApplicationLifetime.StopApplication();
                }
            }
        }
    }
}

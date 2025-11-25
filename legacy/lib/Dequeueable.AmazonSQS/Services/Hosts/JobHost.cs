using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dequeueable.AmazonSQS.Services.Hosts
{
    internal sealed class JobHost(IHostExecutor hostExecutor, IHostApplicationLifetime hostApplicationLifetime, ILogger<JobHost> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await hostExecutor.HandleAsync(stoppingToken);
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

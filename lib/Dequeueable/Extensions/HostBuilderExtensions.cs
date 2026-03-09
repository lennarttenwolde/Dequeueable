using Microsoft.Extensions.DependencyInjection;
using Dequeueable.Services.Hosts;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace Dequeueable.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IHostBuilder"/> to configure and run Dequeueable jobs.
    /// </summary>
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Builds and runs the host as a run-to-completion job. The host will start, process a single message from the queue, and stop.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to run as a job.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
        public static async Task RunJobAsync([NotNull] this IHostBuilder hostBuilder, CancellationToken cancellationToken = default)
        {
            hostBuilder.UseConsoleLifetime();

            var host = hostBuilder.Build();
            await host.StartAsync(cancellationToken);

            try
            {
                await using var scope = host.Services.CreateAsyncScope();
                var executor = scope.ServiceProvider.GetRequiredService<IJobExecutor>();
                await executor.ExecuteAsync(cancellationToken);
            }
            finally
            {
                await host.StopAsync(cancellationToken);
            }
        }
    }
}

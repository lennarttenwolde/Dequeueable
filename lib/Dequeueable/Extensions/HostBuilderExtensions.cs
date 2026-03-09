using Microsoft.Extensions.DependencyInjection;
using Dequeueable.Services.Hosts;
using Microsoft.Extensions.Hosting;

namespace Dequeueable.Extensions
{
    public static class HostBuilderExtensions
    {
        public static async Task RunJobAsync(this IHostBuilder hostBuilder, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(hostBuilder);

            hostBuilder.UseConsoleLifetime();

            var host = hostBuilder.Build();
            await host.StartAsync(cancellationToken);

            await using var scope = host.Services.CreateAsyncScope();
            var executor = scope.ServiceProvider.GetRequiredService<IJobExecutor>();

            await executor.ExecuteAsync(cancellationToken);

            await host.StopAsync(cancellationToken);
        }
    }
}
using Dequeueable.IntegrationTests.TestDataBuilders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Dequeueable.Extensions;

namespace Dequeueable.IntegrationTests
{
#pragma warning disable CA1515 // Consider making public types internal
    public class JobHostFactory<TFunction>
#pragma warning restore CA1515 // Consider making public types internal
        where TFunction : class, IQueueJob
    {
        private readonly IHostBuilder _hostBuilder;
        private readonly Action<Configurations.HostOptions>? _options;

        public JobHostFactory(Action<Configurations.HostOptions>? overrideOptions = null, Action<Configurations.DistributedLockOptions>? distributedLockOptions = null)
        {
            if (overrideOptions is not null)
            {
                _options += overrideOptions;
            }

            _hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    var hostBuilder = services.AddDequeueable<TestFunction>(_options);

                    if (distributedLockOptions is not null)
                    {
                        hostBuilder.WithDistributedLock(distributedLockOptions);
                    }

                    services.AddTransient<IFakeService, FakeService>();
                });
        }

        public IHostBuilder ConfigureTestServices(Action<IServiceCollection> services)
        {
            _hostBuilder.ConfigureServices(services);
            return _hostBuilder;
        }

        public Services.Hosts.IJobExecutor Build()
        {
            var host = _hostBuilder.Build();
            return host.Services.GetRequiredService<Services.Hosts.IJobExecutor>();
        }
    }
}

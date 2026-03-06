using Dequeueable.IntegrationTests.TestDataBuilders;
using Dequeueable.Extentions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dequeueable.IntegrationTests
{
#pragma warning disable CA1515 // Consider making public types internal
    public class JobHostFactory<TFunction>
#pragma warning restore CA1515 // Consider making public types internal
        where TFunction : class, IQueueJob
    {
        private readonly IHostBuilder _hostBuilder;
        private readonly Action<Dequeueable.Configurations.HostOptions>? _options;

        public JobHostFactory(Action<Dequeueable.Configurations.HostOptions>? overrideOptions = null, Action<Configurations.SingletonHostOptions>? singletonHostOptions = null)
        {
            if (overrideOptions is not null)
            {
                _options += overrideOptions;
            }

            _hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    var hostBuilder = services.AddAzureQueueStorageServices<TestFunction>()
                    .RunAsJob(_options);

                    if (singletonHostOptions is not null)
                    {
                        hostBuilder.AsSingleton(singletonHostOptions);
                    }

                    services.AddTransient<IFakeService, FakeService>();
                });
        }

        public IHostBuilder ConfigureTestServices(Action<IServiceCollection> services)
        {
            _hostBuilder.ConfigureServices(services);
            return _hostBuilder;
        }

        public Services.Hosts.IHostExecutor Build()
        {
            var host = _hostBuilder.Build();
            return host.Services.GetRequiredService<Services.Hosts.IHostExecutor>();
        }
    }
}

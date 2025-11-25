using Dequeueable.AmazonSQS.Extentions;
using Dequeueable.AmazonSQS.IntegrationTests.TestDataBuilders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dequeueable.AmazonSQS.IntegrationTests
{
    public class ListenerHostFactory<TFunction>
        where TFunction : class, IAmazonSQSFunction
    {
        private readonly IHostBuilder _hostBuilder;
        private readonly Action<Configurations.ListenerHostOptions>? _options;

        public ListenerHostFactory(Action<Configurations.ListenerHostOptions>? overrideOptions = null, bool runAsSingleton = false)
        {
            if (overrideOptions is not null)
            {
                _options += overrideOptions;
            }

            _hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    var hostBuilder = services.AddAmazonSQSServices<TestFunction>()
                    .RunAsListener(_options);

                    if (runAsSingleton)
                    {
                        hostBuilder.AsSingleton();
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

using Dequeueable.AzureQueueStorage.Configurations;
using Dequeueable.AzureQueueStorage.Extentions;
using Dequeueable.AzureQueueStorage.Models;
using Dequeueable.AzureQueueStorage.Services.Hosts;
using Dequeueable.AzureQueueStorage.Services.Queues;
using Dequeueable.AzureQueueStorage.Services.Singleton;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Dequeueable.AzureQueueStorage.UnitTests.Configurations
{
    public class HostBuilderTests
    {
        private sealed class TestFunction : IAzureQueueFunction
        {
            public Task ExecuteAsync(Message message, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void Given_a_HostBuilder_when_RunAsJob_is_called_then_the_Host_is_registered_correctly()
        {
            // Arrange
            var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddAzureQueueStorageServices<TestFunction>()
                .RunAsJob(options =>
                {
                    options.QueueName = "test";
                    options.ConnectionString = "UseDevelopmentStorage=true";
                });
            });

            // Act
            var host = hostBuilder.Build();

            // Assert
            host.Services.GetRequiredService<IHostedService>().Should().BeOfType<JobHost>();
        }

        [Fact]
        public void Given_a_HostBuilder_when_RunAsJob_is_called_then_IHostOptions_is_registered_correctly()
        {
            // Arrange
            var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddAzureQueueStorageServices<TestFunction>()
                .RunAsJob(options =>
                {
                    options.QueueName = "test";
                    options.ConnectionString = "UseDevelopmentStorage=true";
                });
            });

            // Act
            var host = hostBuilder.Build();

            // Assert
            host.Services.GetRequiredService<IHostOptions>().Should().BeOfType<AzureQueueStorage.Configurations.HostOptions>();
        }

        [Fact]
        public void Given_a_HostBuilder_when_AsSingleton_is_called_then_IQueueMessageExecutor_is_registered_correctly()
        {
            // Arrange
            var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddAzureQueueStorageServices<TestFunction>()
                .RunAsJob(options =>
                {
                    options.QueueName = "test";
                    options.ConnectionString = "UseDevelopmentStorage=true";
                })
                .AsSingleton(opt => opt.Scope = "test");
            });

            // Act
            var host = hostBuilder.Build();

            // Assert
            host.Services.GetRequiredService<IQueueMessageExecutor>().Should().BeOfType<SingletonQueueMessageExecutor>();
        }
    }
}

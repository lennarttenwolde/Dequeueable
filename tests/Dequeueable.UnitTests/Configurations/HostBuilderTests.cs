using Dequeueable.Models;
using Dequeueable.Services.Hosts;
using Dequeueable.Services.Queues;
using Dequeueable.Extentions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Dequeueable.Services.DistributedLock;

namespace Dequeueable.UnitTests.Configurations
{
    public class HostBuilderTests
    {
        private sealed class TestFunction : IQueueJob
        {
            public Task ExecuteAsync(Message message, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void Given_a_HostBuilder_when_AddDequeueable_is_called_then_the_Host_is_registered_correctly()
        {
            // Arrange
            var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddDequeueable<TestFunction>(options =>
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
        public void Given_a_HostBuilder_when_WithDistributedLock_is_called_then_IQueueMessageExecutor_is_registered_correctly()
        {
            // Arrange
            var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddDequeueable<TestFunction>(options =>
                {
                    options.QueueName = "test";
                    options.ConnectionString = "UseDevelopmentStorage=true";
                })
                .WithDistributedLock(opt => opt.Scope = "test");
            });

            // Act
            var host = hostBuilder.Build();

            // Assert
            host.Services.GetRequiredService<IQueueMessageExecutor>().Should().BeOfType<DistributedLockQueueMessageExecutor>();
        }
    }
}

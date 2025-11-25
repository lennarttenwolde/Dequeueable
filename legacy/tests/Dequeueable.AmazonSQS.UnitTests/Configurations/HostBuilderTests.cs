using Amazon;
using Dequeueable.AmazonSQS.Configurations;
using Dequeueable.AmazonSQS.Extentions;
using Dequeueable.AmazonSQS.Factories;
using Dequeueable.AmazonSQS.Models;
using Dequeueable.AmazonSQS.Services.Hosts;
using Dequeueable.AmazonSQS.Services.Queues;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;

namespace Dequeueable.AmazonSQS.UnitTests.Configurations
{
    public class HostBuilderTests
    {
        private sealed class TestFunction : IAmazonSQSFunction
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
                .AddAmazonSQSServices<TestFunction>()
                .RunAsJob(options =>
                {
                    options.QueueUrl = "test";
                });

                var amazonSQSClientFactoryMock = new Mock<IAmazonSQSClientFactory>();
                amazonSQSClientFactoryMock.Setup(c => c.Create()).Returns(new Amazon.SQS.AmazonSQSClient("dummy", "dummy", RegionEndpoint.MECentral1));
                services.AddSingleton(_ => amazonSQSClientFactoryMock.Object);
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
                .AddAmazonSQSServices<TestFunction>()
                .RunAsJob(options =>
                {
                    options.QueueUrl = "test";
                });
            });

            // Act
            var host = hostBuilder.Build();

            // Assert
            host.Services.GetRequiredService<IHostOptions>().Should().BeOfType<AmazonSQS.Configurations.HostOptions>();
        }

        [Fact]
        public void Given_a_HostBuilder_when_RunAsListener_is_called_then_the_Host_is_registered_correctly()
        {
            // Arrange
            var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddAmazonSQSServices<TestFunction>()
                .RunAsListener(options =>
                {
                    options.QueueUrl = "test";
                });

                var amazonSQSClientFactoryMock = new Mock<IAmazonSQSClientFactory>();
                amazonSQSClientFactoryMock.Setup(c => c.Create()).Returns(new Amazon.SQS.AmazonSQSClient("dummy", "dummy", RegionEndpoint.MECentral1));
                services.AddSingleton(_ => amazonSQSClientFactoryMock.Object);
            });

            // Act
            var host = hostBuilder.Build();

            // Assert
            host.Services.GetRequiredService<IHostedService>().Should().BeOfType<QueueListenerHost>();
        }

        [Fact]
        public void Given_a_HostBuilder_when_RunAsListener_is_called_then_IHostOptions_is_registered_correctly()
        {
            // Arrange
            var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddAmazonSQSServices<TestFunction>()
                .RunAsListener(options =>
                {
                    options.QueueUrl = "test";
                });
            });

            // Act
            var host = hostBuilder.Build();

            // Assert
            host.Services.GetRequiredService<IHostOptions>().Should().BeOfType<ListenerHostOptions>();
        }

        [Fact]
        public void Given_a_HostBuilder_when_AsSingleton_is_called_then_IQueueMessageExecutor_is_registered_correctly()
        {
            // Arrange
            var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddAmazonSQSServices<TestFunction>()
                .AsSingleton();
            });

            // Act
            var host = hostBuilder.Build();

            // Assert
            host.Services.GetRequiredService<IQueueMessageExecutor>().Should().BeOfType<SingletonQueueMessageExecutor>();
        }

        [Fact]
        public void Given_a_HostBuilder_when_AsSingleton_is_called_then_HostOptions_AttributeNames_contains_MessageGroupId()
        {
            // Arrange
            var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddAmazonSQSServices<TestFunction>()
                .RunAsJob(options =>
                {
                    options.QueueUrl = "test";
                    options.AttributeNames = new List<string> { "other value" };
                })
                .AsSingleton();
            });

            // Act
            var host = hostBuilder.Build();

            // Assert
            host.Services.GetRequiredService<IOptions<AmazonSQS.Configurations.HostOptions>>().Value.AttributeNames.Should().Contain("MessageGroupId");
        }

        [Fact]
        public void Given_a_HostBuilder_when_AsSingleton_is_called_then_ListenerHostOptions_AttributeNames_contains_MessageGroupId()
        {
            // Arrange
            var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddAmazonSQSServices<TestFunction>()
                .RunAsListener(options =>
                {
                    options.QueueUrl = "test";
                    options.AttributeNames = new List<string> { "other value" };
                })
                .AsSingleton();
            });

            // Act
            var host = hostBuilder.Build();

            // Assert
            host.Services.GetRequiredService<IOptions<ListenerHostOptions>>().Value.AttributeNames.Should().Contain("MessageGroupId");
        }

        [Fact]
        public void Given_a_HostBuilder_when_AsSingleton_is_called_then_ListenerHostOptions_NewBatchThreshold_is_zero()
        {
            // Arrange
            var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddAmazonSQSServices<TestFunction>()
                .RunAsListener(options =>
                {
                    options.QueueUrl = "test";
                    options.NewBatchThreshold = 7;
                })
                .AsSingleton();
            });

            // Act
            var host = hostBuilder.Build();

            // Assert
            host.Services.GetRequiredService<IOptions<ListenerHostOptions>>().Value.NewBatchThreshold.Should().Be(0);
        }
    }
}

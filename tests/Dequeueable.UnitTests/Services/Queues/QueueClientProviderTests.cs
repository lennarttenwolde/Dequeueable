using Azure.Storage.Queues;
using Microsoft.Extensions.Logging.Testing;
using Azure.Identity;
using Dequeueable.Factories;
using Dequeueable.Services.Queues;
using Dequeueable.Configurations;
using Microsoft.Extensions.Options;
using NSubstitute;
using Microsoft.Extensions.Logging;

namespace Dequeueable.UnitTests.Services.Queues
{
    public class QueueClientProviderTests
    {
        [Fact]
        public void Given_a_QueueClientProvider_when_GetQueue_is_called_with_a_ConnectionString_and_QueueName_as_options_then_the_client_is_created_correctly()
        {
            // Arrange
            var options = new HostOptions
            {
                ConnectionString = "unit-test",
                QueueName = "myqueue"
            };
            var logger = new FakeLogger<QueueClientProvider>();
            var factoryMock = Substitute.For<IQueueClientFactory>();

            factoryMock.Create(options.ConnectionString, options.QueueName, options.QueueClientOptions)
                .Returns(Substitute.For<QueueClient>());

            var sut = new QueueClientProvider(factoryMock, Options.Create(options), logger);

            // Act
            sut.GetQueue();

            // Assert
            factoryMock.Received(1).Create(options.ConnectionString, options.QueueName, options.QueueClientOptions);
            Assert.Contains(logger.Collector.GetSnapshot(), e => e.Level == LogLevel.Debug && e.Message.Contains("Authenticate the QueueClient through the ConnectionString", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Given_a_QueueClientProvider_when_GetQueue_is_called_with_an_AuthScheme_as_option_then_the_client_is_created_correctly()
        {
            // Arrange
            var options = new HostOptions
            {
                AuthenticationScheme = new DefaultAzureCredential(),
                AccountName = "testaccount",
                QueueName = "myqueue"
            };
            var logger = new FakeLogger<QueueClientProvider>();
            var factoryMock = Substitute.For<IQueueClientFactory>();

            factoryMock.Create(Arg.Is<Uri>(uri => uri.AbsoluteUri == $"https://{options.AccountName}.queue.core.windows.net/{options.QueueName}"), options.AuthenticationScheme, options.QueueClientOptions)
                .Returns(Substitute.For<QueueClient>());

            var sut = new QueueClientProvider(factoryMock, Options.Create(options), logger);

            // Act
            sut.GetQueue();

            // Assert
            factoryMock.Received(1).Create(Arg.Is<Uri>(uri => uri.AbsoluteUri == $"https://{options.AccountName}.queue.core.windows.net/{options.QueueName}"), options.AuthenticationScheme, options.QueueClientOptions);
            Assert.Contains(logger.Collector.GetSnapshot(), e => e.Level == LogLevel.Debug && e.Message.Contains("Authenticate the QueueClient through Active Directory", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Given_a_QueueClientProvider_when_GetQueue_is_called_with_an_AuthScheme_and_an_invalid_AccountName_then_an_UriFormatException_is_thrown()
        {
            // Arrange
            var options = new HostOptions
            {
                AuthenticationScheme = new DefaultAzureCredential(),
                AccountName = string.Empty,
                QueueName = "myqueue"
            };
            var logger = new FakeLogger<QueueClientProvider>();
            var factoryMock = Substitute.For<IQueueClientFactory>();

            var sut = new QueueClientProvider(factoryMock, Options.Create(options), logger);

            // Act & Assert
            Assert.Throws<UriFormatException>(() => sut.GetQueue());
            Assert.Contains(logger.Collector.GetSnapshot(), e => e.Level == LogLevel.Error && e.Message.Contains("Invalid Uri: The Queue Uri could not be parsed.", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Given_a_QueueClientProvider_when_GetQueue_is_called_with_no_AuthScheme_or_ConnectionString_then_an_InvalidOperationException_is_thrown()
        {
            // Arrange
            var options = new HostOptions
            {
                AuthenticationScheme = null,
                ConnectionString = null,
                QueueName = "myqueue"
            };
            var logger = new FakeLogger<QueueClientProvider>();
            var factoryMock = Substitute.For<IQueueClientFactory>();

            var sut = new QueueClientProvider(factoryMock, Options.Create(options), logger);

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => sut.GetQueue());

            // Assert
            Assert.Equal("No AuthenticationScheme or ConnectionString supplied. Make sure that it is defined in the app settings", ex.Message);
        }

        [Fact]
        public void Given_a_QueueClientProvider_when_GetPoisonQueue_is_called_with_a_ConnectionString_as_options_then_the_client_is_created_correctly()
        {
            // Arrange
            var options = new HostOptions
            {
                ConnectionString = "unit-test",
                QueueName = "myqueue"
            };
            var logger = new FakeLogger<QueueClientProvider>();
            var factoryMock = Substitute.For<IQueueClientFactory>();

            factoryMock.Create(options.ConnectionString, options.PoisonQueueName, options.QueueClientOptions)
                .Returns(Substitute.For<QueueClient>());

            var sut = new QueueClientProvider(factoryMock, Options.Create(options), logger);

            // Act
            sut.GetPoisonQueue();

            // Assert
            factoryMock.Received(1).Create(options.ConnectionString, options.PoisonQueueName, options.QueueClientOptions);
            Assert.Contains(logger.Collector.GetSnapshot(), e => e.Level == LogLevel.Debug && e.Message.Contains("Authenticate the QueueClient through the ConnectionString", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Given_a_QueueClientProvider_when_GetPoisonQueue_is_called_with_an_AuthScheme_as_option_then_the_client_is_created_correctly()
        {
            // Arrange
            var options = new HostOptions
            {
                AuthenticationScheme = new DefaultAzureCredential(),
                AccountName = "testaccount",
                QueueName = "myqueue"
            };
            var logger = new FakeLogger<QueueClientProvider>();
            var factoryMock = Substitute.For<IQueueClientFactory>();

            factoryMock.Create(Arg.Any<Uri>(), options.AuthenticationScheme, options.QueueClientOptions)
                .Returns(Substitute.For<QueueClient>());

            var sut = new QueueClientProvider(factoryMock, Options.Create(options), logger);

            // Act
            sut.GetPoisonQueue();

            // Assert
            factoryMock.Received(1).Create(Arg.Any<Uri>(), options.AuthenticationScheme, options.QueueClientOptions);
            Assert.Contains(logger.Collector.GetSnapshot(), e => e.Level == LogLevel.Debug && e.Message.Contains("Authenticate the QueueClient through Active Directory", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Given_a_QueueClientProvider_when_GetPoisonQueue_is_called_with_an_AuthScheme_and_an_invalid_AccountName_then_an_UriFormatException_is_thrown()
        {
            // Arrange
            var options = new HostOptions
            {
                AuthenticationScheme = new DefaultAzureCredential(),
                AccountName = "invalid uri",
                QueueName = "myqueue"
            };
            var logger = new FakeLogger<QueueClientProvider>();
            var factoryMock = Substitute.For<IQueueClientFactory>();

            var sut = new QueueClientProvider(factoryMock, Options.Create(options), logger);

            // Act & Assert
            Assert.Throws<UriFormatException>(() => sut.GetPoisonQueue());
            Assert.Contains(logger.Collector.GetSnapshot(), e => e.Level == LogLevel.Error && e.Message.Contains("Invalid Uri: The Queue Uri could not be parsed.", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Given_a_QueueClientProvider_when_GetPoisonQueue_is_called_with_no_AuthScheme_or_ConnectionString_then_an_InvalidOperationException_is_thrown()
        {
            // Arrange
            var options = new HostOptions
            {
                AuthenticationScheme = null,
                ConnectionString = null,
                QueueName = "myqueue"
            };
            var logger = new FakeLogger<QueueClientProvider>();
            var factoryMock = Substitute.For<IQueueClientFactory>();

            var sut = new QueueClientProvider(factoryMock, Options.Create(options), logger);

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => sut.GetPoisonQueue());

            // Assert
            Assert.Equal("No AuthenticationScheme or ConnectionString supplied. Make sure that it is defined in the app settings", ex.Message);
        }
    }
}
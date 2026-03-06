using Azure.Storage.Queues;
using Moq;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using FluentAssertions;
using Dequeueable.AzureQueueStorage.Factories;
using Dequeueable.AzureQueueStorage.Services.Queues;
using Dequeueable.AzureQueueStorage.Configurations;

namespace Dequeueable.AzureQueueStorage.UnitTests.Services.Queues
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
            var loggerMock = new Mock<ILogger<QueueClientProvider>>();
            var factoryMock = new Mock<IQueueClientFactory>(MockBehavior.Strict);

            factoryMock.Setup(f => f.Create(options.ConnectionString, options.QueueName, options.QueueClientOptions))
                .Returns(new Mock<QueueClient>().Object)
                .Verifiable();

            var sut = new QueueClientProvider(factoryMock.Object, options, loggerMock.Object);

            // Act
            sut.GetQueue();

            // Assert
            factoryMock.Verify();
            loggerMock.Verify(
                x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Authenticate the QueueClient through the ConnectionString")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
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
            var loggerMock = new Mock<ILogger<QueueClientProvider>>();
            var factoryMock = new Mock<IQueueClientFactory>(MockBehavior.Strict);

            factoryMock.Setup(f => f.Create(It.Is<Uri>(uri => uri.AbsoluteUri == $"https://{options.AccountName}.queue.core.windows.net/{options.QueueName}"), options.AuthenticationScheme, options.QueueClientOptions))
                .Returns(new Mock<QueueClient>().Object)
                .Verifiable();

            var sut = new QueueClientProvider(factoryMock.Object, options, loggerMock.Object);

            // Act
            sut.GetQueue();

            // Assert
            factoryMock.Verify();
            loggerMock.Verify(
                x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Authenticate the QueueClient through Active Directory")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
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
            var loggerMock = new Mock<ILogger<QueueClientProvider>>();
            var factoryMock = new Mock<IQueueClientFactory>(MockBehavior.Strict);

            var sut = new QueueClientProvider(factoryMock.Object, options, loggerMock.Object);

            // Act
            Action act = () => sut.GetQueue();

            // Assert
            act.Should().ThrowExactly<UriFormatException>();
            loggerMock.Verify(
                x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Invalid Uri: The Queue Uri could not be parsed.")),
                It.IsAny<UriFormatException>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
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
            var loggerMock = new Mock<ILogger<QueueClientProvider>>();
            var factoryMock = new Mock<IQueueClientFactory>(MockBehavior.Strict);

            var sut = new QueueClientProvider(factoryMock.Object, options, loggerMock.Object);

            // Act
            Action act = () => sut.GetQueue();

            // Assert
            act.Should().ThrowExactly<InvalidOperationException>().WithMessage("No AuthenticationScheme or ConnectionString supplied. Make sure that it is defined in the app settings");
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
            var loggerMock = new Mock<ILogger<QueueClientProvider>>();
            var factoryMock = new Mock<IQueueClientFactory>(MockBehavior.Strict);

            factoryMock.Setup(f => f.Create(options.ConnectionString, options.PoisonQueueName, options.QueueClientOptions))
                .Returns(new Mock<QueueClient>().Object)
                .Verifiable();

            var sut = new QueueClientProvider(factoryMock.Object, options, loggerMock.Object);

            // Act
            sut.GetPoisonQueue();

            // Assert
            factoryMock.Verify();
            loggerMock.Verify(
                x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Authenticate the QueueClient through the ConnectionString")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
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
            var loggerMock = new Mock<ILogger<QueueClientProvider>>();
            var factoryMock = new Mock<IQueueClientFactory>(MockBehavior.Strict);

            factoryMock.Setup(f => f.Create(It.IsAny<Uri>(), options.AuthenticationScheme, options.QueueClientOptions))
                .Returns(new Mock<QueueClient>().Object)
                .Verifiable();

            var sut = new QueueClientProvider(factoryMock.Object, options, loggerMock.Object);

            // Act
            sut.GetPoisonQueue();

            // Assert
            factoryMock.Verify();
            loggerMock.Verify(
                x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Authenticate the QueueClient through Active Directory")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
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
            var loggerMock = new Mock<ILogger<QueueClientProvider>>();
            var factoryMock = new Mock<IQueueClientFactory>(MockBehavior.Strict);

            var sut = new QueueClientProvider(factoryMock.Object, options, loggerMock.Object);

            // Act
            Action act = () => sut.GetPoisonQueue();

            // Assert
            act.Should().ThrowExactly<UriFormatException>();
            loggerMock.Verify(
                x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Invalid Uri: The Queue Uri could not be parsed.")),
                It.IsAny<UriFormatException>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
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
            var loggerMock = new Mock<ILogger<QueueClientProvider>>();
            var factoryMock = new Mock<IQueueClientFactory>(MockBehavior.Strict);

            var sut = new QueueClientProvider(factoryMock.Object, options, loggerMock.Object);

            // Act
            Action act = () => sut.GetPoisonQueue();

            // Assert
            act.Should().ThrowExactly<InvalidOperationException>().WithMessage("No AuthenticationScheme or ConnectionString supplied. Make sure that it is defined in the app settings");
        }
    }
}

using Dequeueable.AzureQueueStorage.Configurations;
using Dequeueable.AzureQueueStorage.Services.Queues;
using Dequeueable.AzureQueueStorage.UnitTests.TestDataBuilders;
using Microsoft.Extensions.Logging;
using Moq;

namespace Dequeueable.AzureQueueStorage.UnitTests.Services.Queues
{
    public class QueueMessageHandlerTests
    {
        [Fact]
        public async Task Given_a_QueueMessageHandler_when_HandleAsync_is_called_then_message_is_handled_correctly()
        {
            // Arrange
            var message = new MessageTestDataBuilder().Build();
            var queueMessageManagerMock = new Mock<IQueueMessageManager>(MockBehavior.Strict);
            var queueMessageExecutorMock = new Mock<IQueueMessageExecutor>(MockBehavior.Strict);
            var options = new HostOptions();
            var loggerMock = new Mock<ILogger<QueueMessageHandler>>(MockBehavior.Strict);
            var timeProvider = TimeProvider.System;

            queueMessageExecutorMock.Setup(e => e.ExecuteAsync(message, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();
            queueMessageManagerMock.Setup(m => m.DeleteMessageAsync(message, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

            loggerMock.Setup(
                x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Executed message with id '{message.MessageId}' (Succeeded)")),
                null,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)));

            var sut = new QueueMessageHandler(queueMessageExecutorMock.Object, queueMessageManagerMock.Object, timeProvider, loggerMock.Object, options);

            // Act
            await sut.HandleAsync(message, CancellationToken.None);

            // Assert
            queueMessageExecutorMock.Verify();
            queueMessageManagerMock.Verify();
        }

        [Fact]
        public async Task Given_a_QueueMessageHandler_when_HandleAsync_is_called_with_a_message_with_dequeuecount_lower_than_the_max_and_an_exception_occurred_then_the_message_is_enqueued_correctly()
        {
            // Arrange
            var exception = new Exception("test");
            var message = new MessageTestDataBuilder().WithDequeueCount(1).Build();
            var queueMessageManagerMock = new Mock<IQueueMessageManager>(MockBehavior.Strict);
            var queueMessageExecutorMock = new Mock<IQueueMessageExecutor>(MockBehavior.Strict);
            var options = new HostOptions();
            var loggerMock = new Mock<ILogger<QueueMessageHandler>>(MockBehavior.Strict);
            var timeProvider = TimeProvider.System;

            queueMessageExecutorMock.Setup(e => e.ExecuteAsync(message, It.IsAny<CancellationToken>())).ThrowsAsync(exception);
            queueMessageManagerMock.Setup(m => m.EnqueueMessageAsync(message, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

            loggerMock.Setup(
                x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"An error occurred while executing the queue message with id '{message.MessageId}'")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)));

            var sut = new QueueMessageHandler(queueMessageExecutorMock.Object, queueMessageManagerMock.Object, timeProvider, loggerMock.Object, options);

            // Act
            await sut.HandleAsync(message, CancellationToken.None);

            // Assert
            queueMessageExecutorMock.Verify();
            queueMessageManagerMock.Verify();
        }

        [Fact]
        public async Task Given_a_QueueMessageHandler_when_HandleAsync_is_called_with_a_message_with_dequeuecount_higher_than_the_max_and_an_exception_occurred_then_the_message_is_enqueued_correctly()
        {
            // Arrange
            var exception = new Exception("test");
            var message = new MessageTestDataBuilder().WithDequeueCount(3).Build();
            var queueMessageManagerMock = new Mock<IQueueMessageManager>(MockBehavior.Strict);
            var queueMessageExecutorMock = new Mock<IQueueMessageExecutor>(MockBehavior.Strict);
            var options = new HostOptions { MaxDequeueCount = message.DequeueCount };
            var loggerMock = new Mock<ILogger<QueueMessageHandler>>(MockBehavior.Strict);
            var timeProvider = TimeProvider.System;

            queueMessageExecutorMock.Setup(e => e.ExecuteAsync(message, It.IsAny<CancellationToken>())).ThrowsAsync(exception);
            queueMessageManagerMock.Setup(m => m.MoveToPoisonQueueAsync(message, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

            loggerMock.Setup(
                x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"An error occurred while executing the queue message with id '{message.MessageId}'")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)));

            var sut = new QueueMessageHandler(queueMessageExecutorMock.Object, queueMessageManagerMock.Object, timeProvider, loggerMock.Object, options);

            // Act
            await sut.HandleAsync(message, CancellationToken.None);

            // Assert
            queueMessageExecutorMock.Verify();
            queueMessageManagerMock.Verify();
        }

        [Fact]
        public async Task Given_a_QueueMessageHandler_when_HandleAsync_is_called_but_updating_the_visibility_timeout_goes_wrong_then_it_is_handled_correctly()
        {
            // Arrange
            var exception = new Exception("test");
            var message = new MessageTestDataBuilder().WithNextVisibileOn(DateTimeOffset.UtcNow.AddSeconds(2)).WithDequeueCount(2).Build();
            var queueMessageManagerMock = new Mock<IQueueMessageManager>(MockBehavior.Strict);
            var queueMessageExecutorMock = new Mock<IQueueMessageExecutor>(MockBehavior.Strict);
            var options = new HostOptions { MaxDequeueCount = message.DequeueCount + 2 };
            var loggerMock = new Mock<ILogger<QueueMessageHandler>>(MockBehavior.Strict);
            var timeProvider = TimeProvider.System;

            queueMessageExecutorMock.Setup(e => e.ExecuteAsync(message, It.IsAny<CancellationToken>())).Returns(Task.Delay(TimeSpan.FromSeconds(60)));
            queueMessageManagerMock.Setup(m => m.UpdateVisibilityTimeOutAsync(message, It.IsAny<CancellationToken>())).ThrowsAsync(exception);
            queueMessageManagerMock.Setup(m => m.EnqueueMessageAsync(message, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

            loggerMock.Setup(
                x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"An error occurred while executing the queue message with id '{message.MessageId}'")),
                It.IsAny<VisibilityTimeoutException>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)));

            var sut = new QueueMessageHandler(queueMessageExecutorMock.Object, queueMessageManagerMock.Object, timeProvider, loggerMock.Object, options)
            {
                MinimalVisibilityTimeoutDelay = TimeSpan.Zero
            };

            // Act
            await sut.HandleAsync(message, CancellationToken.None);

            // Assert
            queueMessageExecutorMock.Verify();
            queueMessageManagerMock.Verify();
        }
    }
}

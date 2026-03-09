using Dequeueable.Configurations;
using Dequeueable.Services.Queues;
using Dequeueable.UnitTests.TestDataBuilders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Microsoft.Extensions.Logging.Testing;
namespace Dequeueable.UnitTests.Services.Queues
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
            var loggerMock = new FakeLogger<QueueMessageHandler>();
            var timeProvider = TimeProvider.System;

            queueMessageExecutorMock.Setup(e => e.ExecuteAsync(message, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();
            queueMessageManagerMock.Setup(m => m.DeleteMessageAsync(message, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();



            var sut = new QueueMessageHandler(queueMessageExecutorMock.Object, queueMessageManagerMock.Object, timeProvider, loggerMock, Options.Create(options));

            // Act
            await sut.HandleAsync(message, CancellationToken.None);

            // Assert
            queueMessageExecutorMock.Verify();
            queueMessageManagerMock.Verify();
            Assert.Contains(loggerMock.Collector.GetSnapshot(), e => e.Level == LogLevel.Information && e.Message!.Contains(message.MessageId, StringComparison.OrdinalIgnoreCase));
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
            var loggerMock = new FakeLogger<QueueMessageHandler>();
            var timeProvider = TimeProvider.System;

            queueMessageExecutorMock.Setup(e => e.ExecuteAsync(message, It.IsAny<CancellationToken>())).ThrowsAsync(exception);
            queueMessageManagerMock.Setup(m => m.EnqueueMessageAsync(message, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

            var sut = new QueueMessageHandler(queueMessageExecutorMock.Object, queueMessageManagerMock.Object, timeProvider, loggerMock, Options.Create(options));

            // Act
            await sut.HandleAsync(message, CancellationToken.None);

            // Assert
            queueMessageExecutorMock.Verify();
            queueMessageManagerMock.Verify();
            Assert.Contains(loggerMock.Collector.GetSnapshot(), e => e.Level == LogLevel.Error && e.Message.Equals($"An error occurred while executing the queue message with id '{message.MessageId}'", StringComparison.OrdinalIgnoreCase));
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
            var loggerMock = new FakeLogger<QueueMessageHandler>();
            var timeProvider = TimeProvider.System;

            queueMessageExecutorMock.Setup(e => e.ExecuteAsync(message, It.IsAny<CancellationToken>())).ThrowsAsync(exception);
            queueMessageManagerMock.Setup(m => m.MoveToPoisonQueueAsync(message, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

            var sut = new QueueMessageHandler(queueMessageExecutorMock.Object, queueMessageManagerMock.Object, timeProvider, loggerMock, Options.Create(options));

            // Act
            await sut.HandleAsync(message, CancellationToken.None);

            // Assert
            queueMessageExecutorMock.Verify();
            queueMessageManagerMock.Verify();
            Assert.Contains(loggerMock.Collector.GetSnapshot(), e => e.Level == LogLevel.Error && e.Message.Equals($"An error occurred while executing the queue message with id '{message.MessageId}'", StringComparison.OrdinalIgnoreCase));
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
            var loggerMock = new FakeLogger<QueueMessageHandler>();
            var timeProvider = TimeProvider.System;

            queueMessageExecutorMock.Setup(e => e.ExecuteAsync(message, It.IsAny<CancellationToken>())).Returns(Task.Delay(TimeSpan.FromSeconds(60)));
            queueMessageManagerMock.Setup(m => m.UpdateVisibilityTimeOutAsync(message, It.IsAny<CancellationToken>())).ThrowsAsync(exception);
            queueMessageManagerMock.Setup(m => m.EnqueueMessageAsync(message, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

            var sut = new QueueMessageHandler(queueMessageExecutorMock.Object, queueMessageManagerMock.Object, timeProvider, loggerMock, Options.Create(options))
            {
                MinimalVisibilityTimeoutDelay = TimeSpan.Zero
            };

            // Act
            await sut.HandleAsync(message, CancellationToken.None);

            // Assert
            queueMessageExecutorMock.Verify();
            queueMessageManagerMock.Verify();
            Assert.Contains(loggerMock.Collector.GetSnapshot(), e => e.Level == LogLevel.Error && e.Message.Equals($"An error occurred while executing the queue message with id '{message.MessageId}'", StringComparison.OrdinalIgnoreCase));
        }
    }
}

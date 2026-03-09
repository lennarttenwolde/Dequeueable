using Dequeueable.Configurations;
using Dequeueable.Services.Queues;
using Dequeueable.UnitTests.TestDataBuilders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dequeueable.UnitTests.Services.Queues
{
    public class QueueMessageHandlerTests
    {
        [Fact]
        public async Task Given_a_QueueMessageHandler_when_HandleAsync_is_called_then_message_is_handled_correctly()
        {
            // Arrange
            var message = new MessageTestDataBuilder().Build();
            var queueMessageManager = Substitute.For<IQueueMessageManager>();
            var queueMessageExecutor = Substitute.For<IQueueMessageExecutor>();
            var options = new HostOptions();
            var logger = new FakeLogger<QueueMessageHandler>();
            var timeProvider = TimeProvider.System;

            queueMessageExecutor.ExecuteAsync(message, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            queueMessageManager.DeleteMessageAsync(message, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

            var sut = new QueueMessageHandler(queueMessageExecutor, queueMessageManager, timeProvider, logger, Options.Create(options));

            // Act
            await sut.HandleAsync(message, CancellationToken.None);

            // Assert
            await queueMessageExecutor.Received(1).ExecuteAsync(message, Arg.Any<CancellationToken>());
            await queueMessageManager.Received(1).DeleteMessageAsync(message, Arg.Any<CancellationToken>());
            Assert.Contains(logger.Collector.GetSnapshot(), e => e.Level == LogLevel.Information && e.Message.Contains(message.MessageId, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Given_a_QueueMessageHandler_when_HandleAsync_is_called_with_a_message_with_dequeuecount_lower_than_the_max_and_an_exception_occurred_then_the_message_is_enqueued_correctly()
        {
            // Arrange
            var exception = new Exception("test");
            var message = new MessageTestDataBuilder().WithDequeueCount(1).Build();
            var queueMessageManager = Substitute.For<IQueueMessageManager>();
            var queueMessageExecutor = Substitute.For<IQueueMessageExecutor>();
            var options = new HostOptions();
            var logger = new FakeLogger<QueueMessageHandler>();
            var timeProvider = TimeProvider.System;

            queueMessageExecutor.ExecuteAsync(message, Arg.Any<CancellationToken>()).ThrowsAsync(exception);
            queueMessageManager.EnqueueMessageAsync(message, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

            var sut = new QueueMessageHandler(queueMessageExecutor, queueMessageManager, timeProvider, logger, Options.Create(options));

            // Act
            await sut.HandleAsync(message, CancellationToken.None);

            // Assert
            await queueMessageManager.Received(1).EnqueueMessageAsync(message, Arg.Any<CancellationToken>());
            Assert.Contains(logger.Collector.GetSnapshot(), e => e.Level == LogLevel.Error && e.Message.Equals($"An error occurred while executing the queue message with id '{message.MessageId}'", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Given_a_QueueMessageHandler_when_HandleAsync_is_called_with_a_message_with_dequeuecount_higher_than_the_max_and_an_exception_occurred_then_the_message_is_enqueued_correctly()
        {
            // Arrange
            var exception = new Exception("test");
            var message = new MessageTestDataBuilder().WithDequeueCount(3).Build();
            var queueMessageManager = Substitute.For<IQueueMessageManager>();
            var queueMessageExecutor = Substitute.For<IQueueMessageExecutor>();
            var options = new HostOptions { MaxDequeueCount = message.DequeueCount };
            var logger = new FakeLogger<QueueMessageHandler>();
            var timeProvider = TimeProvider.System;

            queueMessageExecutor.ExecuteAsync(message, Arg.Any<CancellationToken>()).ThrowsAsync(exception);
            queueMessageManager.MoveToPoisonQueueAsync(message, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

            var sut = new QueueMessageHandler(queueMessageExecutor, queueMessageManager, timeProvider, logger, Options.Create(options));

            // Act
            await sut.HandleAsync(message, CancellationToken.None);

            // Assert
            await queueMessageManager.Received(1).MoveToPoisonQueueAsync(message, Arg.Any<CancellationToken>());
            Assert.Contains(logger.Collector.GetSnapshot(), e => e.Level == LogLevel.Error && e.Message.Equals($"An error occurred while executing the queue message with id '{message.MessageId}'", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Given_a_QueueMessageHandler_when_HandleAsync_is_called_but_updating_the_visibility_timeout_goes_wrong_then_it_is_handled_correctly()
        {
            // Arrange
            var exception = new Exception("test");
            var message = new MessageTestDataBuilder().WithNextVisibileOn(DateTimeOffset.UtcNow.AddSeconds(2)).WithDequeueCount(2).Build();
            var queueMessageManager = Substitute.For<IQueueMessageManager>();
            var queueMessageExecutor = Substitute.For<IQueueMessageExecutor>();
            var options = new HostOptions { MaxDequeueCount = message.DequeueCount + 2 };
            var logger = new FakeLogger<QueueMessageHandler>();
            var timeProvider = TimeProvider.System;

            queueMessageExecutor.ExecuteAsync(message, Arg.Any<CancellationToken>()).Returns(Task.Delay(TimeSpan.FromSeconds(60)));
            queueMessageManager.UpdateVisibilityTimeOutAsync(message, Arg.Any<CancellationToken>()).ThrowsAsync(exception);
            queueMessageManager.EnqueueMessageAsync(message, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

            var sut = new QueueMessageHandler(queueMessageExecutor, queueMessageManager, timeProvider, logger, Options.Create(options))
            {
                MinimalVisibilityTimeoutDelay = TimeSpan.Zero
            };

            // Act
            await sut.HandleAsync(message, CancellationToken.None);

            // Assert
            await queueMessageManager.Received(1).EnqueueMessageAsync(message, Arg.Any<CancellationToken>());
            Assert.Contains(logger.Collector.GetSnapshot(), e => e.Level == LogLevel.Error && e.Message.Equals($"An error occurred while executing the queue message with id '{message.MessageId}'", StringComparison.OrdinalIgnoreCase));
        }
    }
}
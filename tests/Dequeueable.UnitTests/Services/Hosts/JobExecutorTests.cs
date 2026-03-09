using Dequeueable.Models;
using Dequeueable.Services.Hosts;
using Dequeueable.Services.Queues;
using Dequeueable.UnitTests.TestDataBuilders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace Dequeueable.UnitTests.Services.Hosts
{
    public class JobExecutorTests
    {
        [Fact]
        public async Task Given_a_JobExecutor_when_ExecuteAsync_is_called_but_no_message_is_retrieved_then_the_handler_is_not_called()
        {
            // Arrange
            var queueMessageManager = Substitute.For<IQueueMessageManager>();
            var queueMessageHandler = Substitute.For<IQueueMessageHandler>();
            var logger = new FakeLogger<JobExecutor>();

            queueMessageManager.RetrieveMessageAsync(Arg.Any<CancellationToken>()).Returns((Message?)null);

            var sut = new JobExecutor(queueMessageManager, queueMessageHandler, logger);

            // Act
            await sut.ExecuteAsync(CancellationToken.None);

            // Assert
            queueMessageHandler.DidNotReceive();
            Assert.Contains(logger.Collector.GetSnapshot(), e => e.Level == LogLevel.Debug && e.Message.Equals("No messages found", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Given_a_JobExecutor_when_ExecuteAsync_is_called_and_message_is_retrieved_then_the_handler_is_called_correctly()
        {
            // Arrange
            var message = new MessageTestDataBuilder().WithmessageId("1").Build();
            var queueMessageManager = Substitute.For<IQueueMessageManager>();
            var queueMessageHandler = Substitute.For<IQueueMessageHandler>();
            var logger = new FakeLogger<JobExecutor>();

            queueMessageManager.RetrieveMessageAsync(Arg.Any<CancellationToken>()).Returns(message);
            queueMessageHandler.HandleAsync(Arg.Is<Message>(m => m.MessageId == message.MessageId), CancellationToken.None).Returns(Task.CompletedTask);

            var sut = new JobExecutor(queueMessageManager, queueMessageHandler, logger);

            // Act
            await sut.ExecuteAsync(CancellationToken.None);

            // Assert
            await queueMessageHandler.Received(1).HandleAsync(Arg.Is<Message>(m => m.MessageId == message.MessageId), Arg.Any<CancellationToken>());
        }
    }
}

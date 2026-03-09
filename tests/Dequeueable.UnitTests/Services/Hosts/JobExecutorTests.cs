using Dequeueable.Models;
using Dequeueable.Services.Hosts;
using Dequeueable.Services.Queues;
using Dequeueable.UnitTests.TestDataBuilders;
using Microsoft.Extensions.Logging;
using Moq;

namespace Dequeueable.UnitTests.Services.Hosts
{
    public class JobExecutorTests
    {

        [Fact]
        public async Task Given_a_JobExecutor_when_ExecuteAsync_is_called_but_no_messages_are_retrieved_then_the_handler_is_not_called()
        {
            // Arrange
            var queueMessageManagerMock = new Mock<IQueueMessageManager>(MockBehavior.Strict);
            var queueMessageHandlerMock = new Mock<IQueueMessageHandler>(MockBehavior.Strict);
            var loggerMock = new Mock<ILogger<JobExecutor>>(MockBehavior.Strict);

            queueMessageManagerMock.Setup(m => m.RetrieveMessageAsync(It.IsAny<CancellationToken>())).ReturnsAsync((Message?)null);

            loggerMock.Setup(
                x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No messages found")),
                null,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)));

            var sut = new JobExecutor(queueMessageManagerMock.Object, queueMessageHandlerMock.Object, loggerMock.Object);

            // Act
            await sut.ExecuteAsync(CancellationToken.None);

            // Assert
            queueMessageHandlerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Given_a_JobExecutor_when_ExecuteAsync_is_called_and_messages_are_retrieved_then_the_handler_is_called_correctly()
        {
            // Arrange
            var message = new MessageTestDataBuilder().WithmessageId("1").Build();
            var queueMessageManagerMock = new Mock<IQueueMessageManager>(MockBehavior.Strict);
            var queueMessageHandlerMock = new Mock<IQueueMessageHandler>(MockBehavior.Strict);
            var loggerMock = new Mock<ILogger<JobExecutor>>(MockBehavior.Strict);

            queueMessageManagerMock.Setup(m => m.RetrieveMessageAsync(It.IsAny<CancellationToken>())).ReturnsAsync(message);
            queueMessageHandlerMock.Setup(h => h.HandleAsync(It.Is<Message>(m => m.MessageId == message.MessageId), CancellationToken.None)).Returns(Task.CompletedTask);

            var sut = new JobExecutor(queueMessageManagerMock.Object, queueMessageHandlerMock.Object, loggerMock.Object);

            // Act
            await sut.ExecuteAsync(CancellationToken.None);

            // Assert
            queueMessageHandlerMock.Verify(e => e.HandleAsync(It.Is<Message>(m => m.MessageId == message.MessageId), It.IsAny<CancellationToken>()), Times.Exactly(1));
        }
    }
}

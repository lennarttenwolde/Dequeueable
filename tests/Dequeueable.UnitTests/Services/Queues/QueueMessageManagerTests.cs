using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Dequeueable.AzureQueueStorage.Configurations;
using Dequeueable.AzureQueueStorage.Services.Queues;
using Dequeueable.AzureQueueStorage.UnitTests.TestDataBuilders;
using FluentAssertions;
using Moq;

namespace Dequeueable.AzureQueueStorage.UnitTests.Services.Queues
{
    public class QueueMessageManagerTests
    {
        [Fact]
        public async Task Given_a_QueueMessageManager_when_RetrieveMessagesAsync_is_called_then_messages_are_retrieved_correctly()
        {
            // Arrange
            var options = new HostOptions();
            var queueClientProviderMock = new Mock<IQueueClientProvider>(MockBehavior.Strict);
            var queueClientFake = new Mock<QueueClient>(MockBehavior.Strict);
            var queueMessages = new[] { QueuesModelFactory.QueueMessage("id", "pop", BinaryData.FromString("message"), 2) };

            var responseFake = new Mock<Response<QueueMessage[]>>();
            responseFake.SetupGet(r => r.Value).Returns(queueMessages);
            queueClientFake.Setup(c => c.ReceiveMessagesAsync(options.BatchSize, TimeSpan.FromSeconds(options.VisibilityTimeoutInSeconds), It.IsAny<CancellationToken>())).ReturnsAsync(responseFake.Object);
            queueClientProviderMock.Setup(c => c.GetQueue()).Returns(queueClientFake.Object);
            queueClientProviderMock.Setup(c => c.GetPoisonQueue()).Returns(queueClientFake.Object);

            var sut = new QueueMessageManager(queueClientProviderMock.Object, options);

            // Act
            var messages = await sut.RetrieveMessagesAsync(CancellationToken.None);

            // Assert
            messages.Should().HaveSameCount(queueMessages);
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_RetrieveMessagesAsync_is_called_and_a_404_exception_occurred_then_the_queue_is_created_and_the_messages_are_retrieved_correctly()
        {
            // Arrange
            var options = new HostOptions();
            var queueClientProviderMock = new Mock<IQueueClientProvider>(MockBehavior.Strict);
            var queueClientFake = new Mock<QueueClient>(MockBehavior.Strict);
            var queueMessages = new[] { QueuesModelFactory.QueueMessage("id", "pop", BinaryData.FromString("message"), 2) };
            var succeededResponseFake = new Mock<Response<QueueMessage[]>>();

            succeededResponseFake.SetupGet(r => r.Value).Returns(queueMessages);

            var requestFailedException = new RequestFailedException(404, "");
            queueClientFake.SetupSequence(c => c.ReceiveMessagesAsync(options.BatchSize, TimeSpan.FromSeconds(options.VisibilityTimeoutInSeconds), It.IsAny<CancellationToken>()))
                .ThrowsAsync(requestFailedException)
                .ReturnsAsync(succeededResponseFake.Object);

            queueClientFake.Setup(c => c.CreateAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Mock<Response>().Object);
            queueClientProviderMock.Setup(c => c.GetQueue()).Returns(queueClientFake.Object);
            queueClientProviderMock.Setup(c => c.GetPoisonQueue()).Returns(queueClientFake.Object);

            var sut = new QueueMessageManager(queueClientProviderMock.Object, options);

            // Act
            var messages = await sut.RetrieveMessagesAsync(CancellationToken.None);

            // Assert
            messages.Should().HaveSameCount(queueMessages);
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_RetrieveMessagesAsync_is_called_and_a_404_exception_occurred_and_an_exception_occurred_when_creating_the_queue_then_an_exception_is_thrown()
        {
            // Arrange
            var options = new HostOptions();
            var queueClientProviderMock = new Mock<IQueueClientProvider>(MockBehavior.Strict);
            var queueClientFake = new Mock<QueueClient>(MockBehavior.Strict);
            var queueMessages = new[] { QueuesModelFactory.QueueMessage("id", "pop", BinaryData.FromString("message"), 2) };
            var succeededresponseFake = new Mock<Response<QueueMessage[]>>();

            succeededresponseFake.SetupGet(r => r.Value).Returns(queueMessages);

            var requestFailedException = new RequestFailedException(404, "");
            queueClientFake.Setup(c => c.ReceiveMessagesAsync(options.BatchSize, TimeSpan.FromSeconds(options.VisibilityTimeoutInSeconds), It.IsAny<CancellationToken>()))
                .ThrowsAsync(requestFailedException);

            queueClientFake.Setup(c => c.CreateAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>())).ThrowsAsync(new RequestFailedException(409, "some conflict"));
            queueClientProviderMock.Setup(c => c.GetQueue()).Returns(queueClientFake.Object);
            queueClientProviderMock.Setup(c => c.GetPoisonQueue()).Returns(queueClientFake.Object);

            var sut = new QueueMessageManager(queueClientProviderMock.Object, options);

            // Act
            Func<Task> act = () => sut.RetrieveMessagesAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<RequestFailedException>();
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_UpdateVisibilityTimeOutAsync_is_called_then_the_message_is_updated_correctly()
        {
            // Arrange
            var message = new MessageTestDataBuilder().Build();
            var options = new HostOptions();
            var queueClientProviderMock = new Mock<IQueueClientProvider>(MockBehavior.Strict);
            var queueClientFake = new Mock<QueueClient>(MockBehavior.Strict);
            var responseFake = new Mock<Response<UpdateReceipt>>();
            var updateReceiptFake = new Mock<UpdateReceipt>();

            responseFake.SetupGet(r => r.Value).Returns(updateReceiptFake.Object);
            queueClientFake.Setup(c => c.UpdateMessageAsync(message.MessageId, message.PopReceipt, (string?)null, TimeSpan.FromSeconds(options.VisibilityTimeoutInSeconds), It.IsAny<CancellationToken>())).ReturnsAsync(responseFake.Object);
            queueClientProviderMock.Setup(c => c.GetQueue()).Returns(queueClientFake.Object);
            queueClientProviderMock.Setup(c => c.GetPoisonQueue()).Returns(queueClientFake.Object);

            var sut = new QueueMessageManager(queueClientProviderMock.Object, options);

            // Act
            var nextVisibleOn = await sut.UpdateVisibilityTimeOutAsync(message, CancellationToken.None);

            // Assert
            nextVisibleOn.Should().Be(updateReceiptFake.Object.NextVisibleOn);
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_DeleteMessageAsync_is_called_then_message_is_deleted_correctly()
        {
            // Arrange
            var message = new MessageTestDataBuilder().Build();
            var options = new HostOptions();
            var queueClientProviderMock = new Mock<IQueueClientProvider>(MockBehavior.Strict);
            var queueClientFake = new Mock<QueueClient>(MockBehavior.Strict);
            var responseFake = new Mock<Response>();

            queueClientFake.Setup(c => c.DeleteMessageAsync(message.MessageId, message.PopReceipt, It.IsAny<CancellationToken>())).ReturnsAsync(responseFake.Object).Verifiable();
            queueClientProviderMock.Setup(c => c.GetQueue()).Returns(queueClientFake.Object);
            queueClientProviderMock.Setup(c => c.GetPoisonQueue()).Returns(queueClientFake.Object);

            var sut = new QueueMessageManager(queueClientProviderMock.Object, options);

            // Act
            await sut.DeleteMessageAsync(message, CancellationToken.None);

            // Assert
            queueClientFake.Verify();
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_DeleteMessageAsync_is_called_and_a_404_exception_occurres_then_it_is_handled_correctly()
        {
            // Arrange
            var message = new MessageTestDataBuilder().Build();
            var options = new HostOptions();
            var queueClientProviderMock = new Mock<IQueueClientProvider>(MockBehavior.Strict);
            var queueClientFake = new Mock<QueueClient>(MockBehavior.Strict);
            var exception = new RequestFailedException(404, "test exception");

            queueClientFake.Setup(c => c.DeleteMessageAsync(message.MessageId, message.PopReceipt, It.IsAny<CancellationToken>())).ThrowsAsync(exception);
            queueClientProviderMock.Setup(c => c.GetQueue()).Returns(queueClientFake.Object);
            queueClientProviderMock.Setup(c => c.GetPoisonQueue()).Returns(queueClientFake.Object);

            var sut = new QueueMessageManager(queueClientProviderMock.Object, options);

            // Act
            Func<Task> act = () => sut.DeleteMessageAsync(message, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_DeleteMessageAsync_is_called_and_an_exception_with_status_that_is_NOT_404_occurres_then_it_an_exception_is_thrown()
        {
            // Arrange
            var message = new MessageTestDataBuilder().Build();
            var options = new HostOptions();
            var queueClientProviderMock = new Mock<IQueueClientProvider>(MockBehavior.Strict);
            var queueClientFake = new Mock<QueueClient>(MockBehavior.Strict);
            var exception = new RequestFailedException(409, "test exception");

            queueClientFake.Setup(c => c.DeleteMessageAsync(message.MessageId, message.PopReceipt, It.IsAny<CancellationToken>())).ThrowsAsync(exception);
            queueClientProviderMock.Setup(c => c.GetQueue()).Returns(queueClientFake.Object);
            queueClientProviderMock.Setup(c => c.GetPoisonQueue()).Returns(queueClientFake.Object);

            var sut = new QueueMessageManager(queueClientProviderMock.Object, options);

            // Act
            Func<Task> act = () => sut.DeleteMessageAsync(message, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<RequestFailedException>();
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_EnqueueMessageAsync_is_called_then_the_message_is_updated_correctly()
        {
            // Arrange
            var message = new MessageTestDataBuilder().Build();
            var options = new HostOptions();
            var queueClientProviderMock = new Mock<IQueueClientProvider>(MockBehavior.Strict);
            var queueClientFake = new Mock<QueueClient>(MockBehavior.Strict);
            var responseFake = new Mock<Response<UpdateReceipt>>();
            var updateReceiptFake = new Mock<UpdateReceipt>();

            responseFake.SetupGet(r => r.Value).Returns(updateReceiptFake.Object);
            queueClientFake.Setup(c => c.UpdateMessageAsync(message.MessageId, message.PopReceipt, message.Body, TimeSpan.Zero, It.IsAny<CancellationToken>())).ReturnsAsync(responseFake.Object).Verifiable();
            queueClientProviderMock.Setup(c => c.GetQueue()).Returns(queueClientFake.Object);
            queueClientProviderMock.Setup(c => c.GetPoisonQueue()).Returns(queueClientFake.Object);

            var sut = new QueueMessageManager(queueClientProviderMock.Object, options);

            // Act
            await sut.EnqueueMessageAsync(message, CancellationToken.None);

            // Assert
            queueClientFake.Verify();
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_MoveToPoisonQueueAsync_is_called_then_the_message_is_updated_correctly()
        {
            // Arrange
            var message = new MessageTestDataBuilder().Build();
            var options = new HostOptions();
            var queueClientProviderMock = new Mock<IQueueClientProvider>(MockBehavior.Strict);
            var queueClientFake = new Mock<QueueClient>(MockBehavior.Strict);
            var poisonqueueClientFake = new Mock<QueueClient>(MockBehavior.Strict);

            poisonqueueClientFake.Setup(c => c.SendMessageAsync(message.Body, null, null, It.IsAny<CancellationToken>())).ReturnsAsync(new Mock<Response<SendReceipt>>().Object).Verifiable();
            queueClientFake.Setup(c => c.DeleteMessageAsync(message.MessageId, message.PopReceipt, It.IsAny<CancellationToken>())).ReturnsAsync(new Mock<Response>().Object).Verifiable();

            queueClientProviderMock.Setup(c => c.GetQueue()).Returns(queueClientFake.Object);
            queueClientProviderMock.Setup(c => c.GetPoisonQueue()).Returns(poisonqueueClientFake.Object);

            var sut = new QueueMessageManager(queueClientProviderMock.Object, options);

            // Act
            await sut.MoveToPoisonQueueAsync(message, CancellationToken.None);

            // Assert
            queueClientFake.Verify();
            poisonqueueClientFake.Verify();
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_MoveToPoisonQueueAsync_is_called_and_404_exception_is_thrown_then_it_is_handled_correctly()
        {
            // Arrange
            var message = new MessageTestDataBuilder().Build();
            var options = new HostOptions();
            var queueClientProviderMock = new Mock<IQueueClientProvider>(MockBehavior.Strict);
            var queueClientFake = new Mock<QueueClient>(MockBehavior.Strict);
            var poisonqueueClientFake = new Mock<QueueClient>(MockBehavior.Strict);

            poisonqueueClientFake.SetupSequence(c => c.SendMessageAsync(message.Body, null, null, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(404, "queue not found)"))
                .ReturnsAsync(new Mock<Response<SendReceipt>>().Object);
            queueClientFake.Setup(c => c.DeleteMessageAsync(message.MessageId, message.PopReceipt, It.IsAny<CancellationToken>())).ReturnsAsync(new Mock<Response>().Object).Verifiable();
            poisonqueueClientFake.Setup(c => c.CreateAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Mock<Response>().Object);

            queueClientProviderMock.Setup(c => c.GetQueue()).Returns(queueClientFake.Object);
            queueClientProviderMock.Setup(c => c.GetPoisonQueue()).Returns(poisonqueueClientFake.Object);

            var sut = new QueueMessageManager(queueClientProviderMock.Object, options);

            // Act
            await sut.MoveToPoisonQueueAsync(message, CancellationToken.None);

            // Assert
            queueClientFake.Verify();
            poisonqueueClientFake.Verify();
        }
    }
}

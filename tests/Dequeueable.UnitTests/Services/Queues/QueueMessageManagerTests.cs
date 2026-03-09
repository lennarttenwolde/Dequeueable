using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Dequeueable.Configurations;
using Dequeueable.Services.Queues;
using Dequeueable.UnitTests.TestDataBuilders;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dequeueable.UnitTests.Services.Queues
{
    public class QueueMessageManagerTests
    {
        [Fact]
        public async Task Given_a_QueueMessageManager_when_RetrieveMessageAsync_is_called_then_message_is_retrieved_correctly()
        {
            // Arrange
            var options = new HostOptions();
            var queueClientProvider = Substitute.For<IQueueClientProvider>();
            var queueClient = Substitute.For<QueueClient>();
            var queueMessages = new[] { QueuesModelFactory.QueueMessage("id", "pop", BinaryData.FromString("message"), 2) };

            var response = Substitute.For<Response<QueueMessage[]>>();
            response.Value.Returns(queueMessages);
            queueClient.ReceiveMessagesAsync(1, TimeSpan.FromSeconds(options.VisibilityTimeoutInSeconds), Arg.Any<CancellationToken>()).Returns(response);
            queueClientProvider.GetQueue().Returns(queueClient);
            queueClientProvider.GetPoisonQueue().Returns(queueClient);

            var sut = new QueueMessageManager(queueClientProvider, Options.Create(options));

            // Act
            var message = await sut.RetrieveMessageAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(message);
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_RetrieveMessageAsync_is_called_and_a_404_exception_occurred_then_the_queue_is_created_and_the_message_is_handled()
        {
            // Arrange
            var options = new HostOptions();
            var queueClientProvider = Substitute.For<IQueueClientProvider>();
            var queueClient = Substitute.For<QueueClient>();
            var queueMessages = new[] { QueuesModelFactory.QueueMessage("id", "pop", BinaryData.FromString("message"), 2) };

            var response = Substitute.For<Response<QueueMessage[]>>();
            response.Value.Returns(queueMessages);

            queueClient.ReceiveMessagesAsync(1, TimeSpan.FromSeconds(options.VisibilityTimeoutInSeconds), Arg.Any<CancellationToken>())
                .Returns(_ => throw new RequestFailedException(404, ""), _ => response);
            queueClient.CreateAsync(Arg.Any<IDictionary<string, string>>(), Arg.Any<CancellationToken>())
                .Returns(Substitute.For<Response>());

            queueClientProvider.GetQueue().Returns(queueClient);
            queueClientProvider.GetPoisonQueue().Returns(queueClient);

            var sut = new QueueMessageManager(queueClientProvider, Options.Create(options));

            // Act
            var message = await sut.RetrieveMessageAsync(CancellationToken.None);

            // Assert
            Assert.Equal(queueMessages[0].MessageId, message!.MessageId);
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_RetrieveMessageAsync_is_called_and_a_404_exception_occurred_and_an_exception_occurred_when_creating_the_queue_then_an_exception_is_thrown()
        {
            // Arrange
            var options = new HostOptions();
            var queueClientProvider = Substitute.For<IQueueClientProvider>();
            var queueClient = Substitute.For<QueueClient>();

            queueClient.ReceiveMessagesAsync(1, TimeSpan.FromSeconds(options.VisibilityTimeoutInSeconds), Arg.Any<CancellationToken>())
                .ThrowsAsync(new RequestFailedException(404, ""));
            queueClient.CreateAsync(Arg.Any<IDictionary<string, string>>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new RequestFailedException(409, "some conflict"));

            queueClientProvider.GetQueue().Returns(queueClient);
            queueClientProvider.GetPoisonQueue().Returns(queueClient);

            var sut = new QueueMessageManager(queueClientProvider, Options.Create(options));

            // Act & Assert
            await Assert.ThrowsAsync<RequestFailedException>(() => sut.RetrieveMessageAsync(CancellationToken.None));
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_UpdateVisibilityTimeOutAsync_is_called_then_the_message_is_updated_correctly()
        {
            // Arrange
            var message = new MessageTestDataBuilder().Build();
            var options = new HostOptions();
            var queueClientProvider = Substitute.For<IQueueClientProvider>();
            var queueClient = Substitute.For<QueueClient>();

            var updateReceipt = QueuesModelFactory.UpdateReceipt("newPopReceipt", DateTimeOffset.UtcNow.AddMinutes(5));
            var response = Substitute.For<Response<UpdateReceipt>>();
            response.Value.Returns(updateReceipt);

            queueClient.UpdateMessageAsync(message.MessageId, message.PopReceipt, (string?)null, TimeSpan.FromSeconds(options.VisibilityTimeoutInSeconds), Arg.Any<CancellationToken>())
                .Returns(response);
            queueClientProvider.GetQueue().Returns(queueClient);
            queueClientProvider.GetPoisonQueue().Returns(queueClient);

            var sut = new QueueMessageManager(queueClientProvider, Options.Create(options));

            // Act
            var nextVisibleOn = await sut.UpdateVisibilityTimeOutAsync(message, CancellationToken.None);

            // Assert
            Assert.Equal(updateReceipt.NextVisibleOn, nextVisibleOn);
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_DeleteMessageAsync_is_called_then_message_is_deleted_correctly()
        {
            // Arrange
            var message = new MessageTestDataBuilder().Build();
            var options = new HostOptions();
            var queueClientProvider = Substitute.For<IQueueClientProvider>();
            var queueClient = Substitute.For<QueueClient>();

            queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, Arg.Any<CancellationToken>())
                .Returns(Substitute.For<Response>());
            queueClientProvider.GetQueue().Returns(queueClient);
            queueClientProvider.GetPoisonQueue().Returns(queueClient);

            var sut = new QueueMessageManager(queueClientProvider, Options.Create(options));

            // Act
            await sut.DeleteMessageAsync(message, CancellationToken.None);

            // Assert
            await queueClient.Received(1).DeleteMessageAsync(message.MessageId, message.PopReceipt, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_DeleteMessageAsync_is_called_and_a_404_exception_occurres_then_it_is_handled_correctly()
        {
            // Arrange
            var message = new MessageTestDataBuilder().Build();
            var options = new HostOptions();
            var queueClientProvider = Substitute.For<IQueueClientProvider>();
            var queueClient = Substitute.For<QueueClient>();

            queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, Arg.Any<CancellationToken>())
                .ThrowsAsync(new RequestFailedException(404, "test exception"));
            queueClientProvider.GetQueue().Returns(queueClient);
            queueClientProvider.GetPoisonQueue().Returns(queueClient);

            var sut = new QueueMessageManager(queueClientProvider, Options.Create(options));

            // Act & Assert
            await sut.DeleteMessageAsync(message, CancellationToken.None); // should not throw
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_DeleteMessageAsync_is_called_and_an_exception_with_status_that_is_NOT_404_occurres_then_it_an_exception_is_thrown()
        {
            // Arrange
            var message = new MessageTestDataBuilder().Build();
            var options = new HostOptions();
            var queueClientProvider = Substitute.For<IQueueClientProvider>();
            var queueClient = Substitute.For<QueueClient>();

            queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, Arg.Any<CancellationToken>())
                .ThrowsAsync(new RequestFailedException(409, "test exception"));
            queueClientProvider.GetQueue().Returns(queueClient);
            queueClientProvider.GetPoisonQueue().Returns(queueClient);

            var sut = new QueueMessageManager(queueClientProvider, Options.Create(options));

            // Act & Assert
            await Assert.ThrowsAsync<RequestFailedException>(() => sut.DeleteMessageAsync(message, CancellationToken.None));
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_EnqueueMessageAsync_is_called_then_the_message_is_updated_correctly()
        {
            // Arrange
            var message = new MessageTestDataBuilder().Build();
            var options = new HostOptions();
            var queueClientProvider = Substitute.For<IQueueClientProvider>();
            var queueClient = Substitute.For<QueueClient>();

            queueClient.UpdateMessageAsync(message.MessageId, message.PopReceipt, message.Body, TimeSpan.Zero, Arg.Any<CancellationToken>())
                .Returns(Substitute.For<Response<UpdateReceipt>>());
            queueClientProvider.GetQueue().Returns(queueClient);
            queueClientProvider.GetPoisonQueue().Returns(queueClient);

            var sut = new QueueMessageManager(queueClientProvider, Options.Create(options));

            // Act
            await sut.EnqueueMessageAsync(message, CancellationToken.None);

            // Assert
            await queueClient.Received(1).UpdateMessageAsync(message.MessageId, message.PopReceipt, message.Body, TimeSpan.Zero, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_MoveToPoisonQueueAsync_is_called_then_the_message_is_updated_correctly()
        {
            // Arrange
            var message = new MessageTestDataBuilder().Build();
            var options = new HostOptions();
            var queueClientProvider = Substitute.For<IQueueClientProvider>();
            var queueClient = Substitute.For<QueueClient>();
            var poisonQueueClient = Substitute.For<QueueClient>();

            poisonQueueClient.SendMessageAsync(message.Body, null, null, Arg.Any<CancellationToken>())
                .Returns(Substitute.For<Response<SendReceipt>>());
            queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, Arg.Any<CancellationToken>())
                .Returns(Substitute.For<Response>());

            queueClientProvider.GetQueue().Returns(queueClient);
            queueClientProvider.GetPoisonQueue().Returns(poisonQueueClient);

            var sut = new QueueMessageManager(queueClientProvider, Options.Create(options));

            // Act
            await sut.MoveToPoisonQueueAsync(message, CancellationToken.None);

            // Assert
            await poisonQueueClient.Received(1).SendMessageAsync(message.Body, null, null, Arg.Any<CancellationToken>());
            await queueClient.Received(1).DeleteMessageAsync(message.MessageId, message.PopReceipt, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_MoveToPoisonQueueAsync_is_called_and_404_exception_is_thrown_then_it_is_handled_correctly()
        {
            // Arrange
            var message = new MessageTestDataBuilder().Build();
            var options = new HostOptions();
            var queueClientProvider = Substitute.For<IQueueClientProvider>();
            var queueClient = Substitute.For<QueueClient>();
            var poisonQueueClient = Substitute.For<QueueClient>();

            poisonQueueClient.SendMessageAsync(message.Body, null, null, Arg.Any<CancellationToken>())
                .Returns(_ => throw new RequestFailedException(404, "queue not found"), _ => Substitute.For<Response<SendReceipt>>());
            queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, Arg.Any<CancellationToken>())
                .Returns(Substitute.For<Response>());
            poisonQueueClient.CreateAsync(Arg.Any<IDictionary<string, string>>(), Arg.Any<CancellationToken>())
                .Returns(Substitute.For<Response>());

            queueClientProvider.GetQueue().Returns(queueClient);
            queueClientProvider.GetPoisonQueue().Returns(poisonQueueClient);

            var sut = new QueueMessageManager(queueClientProvider, Options.Create(options));

            // Act
            await sut.MoveToPoisonQueueAsync(message, CancellationToken.None);

            // Assert
            await poisonQueueClient.Received(2).SendMessageAsync(message.Body, null, null, Arg.Any<CancellationToken>());
            await queueClient.Received(1).DeleteMessageAsync(message.MessageId, message.PopReceipt, Arg.Any<CancellationToken>());
        }
    }
}
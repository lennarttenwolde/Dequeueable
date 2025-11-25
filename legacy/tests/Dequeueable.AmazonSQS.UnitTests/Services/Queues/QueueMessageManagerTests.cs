using Amazon.SQS;
using Amazon.SQS.Model;
using Dequeueable.AmazonSQS.Configurations;
using Dequeueable.AmazonSQS.Factories;
using Dequeueable.AmazonSQS.Services.Queues;
using Dequeueable.AmazonSQS.UnitTests.TestDataBuilders;
using FluentAssertions;
using Moq;

namespace Dequeueable.AmazonSQS.UnitTests.Services.Queues
{
    public class QueueMessageManagerTests
    {
        [Fact]
        public async Task Given_a_QueueMessageManager_when_RetrieveMessagesAsync_is_called_then_messages_are_retrieved_correctly()
        {
            // Arrange
            var options = new HostOptions();
            var amazonSQSClientFactoryMock = new Mock<IAmazonSQSClientFactory>(MockBehavior.Strict);
            var clientFake = new Mock<AmazonSQSClient>("TESTKEY", "TESTSECRET", Amazon.RegionEndpoint.EUCentral1); ;
            var fakeResponse = new ReceiveMessageResponse
            {
                Messages = new List<Message> { new Message() }
            };

            clientFake.Setup(r => r.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(fakeResponse);

            amazonSQSClientFactoryMock.Setup(e => e.Create()).Returns(clientFake.Object);
            var sut = new QueueMessageManager(amazonSQSClientFactoryMock.Object, options);

            // Act
            var messages = await sut.RetrieveMessagesAsync(CancellationToken.None);

            // Assert
            messages.Should().HaveSameCount(fakeResponse.Messages);
            messages.Should().AllSatisfy(m => m.NextVisibleOn.Should().BeCloseTo(DateTimeOffset.UtcNow.Add(TimeSpan.FromSeconds(options.VisibilityTimeoutInSeconds)), TimeSpan.FromMilliseconds(100)));
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_DeleteMessageAsync_is_called_then_the_message_is_deleted_correctly()
        {
            // Arrange
            var options = new HostOptions();
            var amazonSQSClientFactoryMock = new Mock<IAmazonSQSClientFactory>(MockBehavior.Strict);
            var clientFake = new Mock<AmazonSQSClient>("TESTKEY", "TESTSECRET", Amazon.RegionEndpoint.EUCentral1); ;
            var message = new MessageTestDataBuilder().Build();
            clientFake.Setup(r => r.DeleteMessageAsync(options.QueueUrl, message.ReceiptHandle, It.IsAny<CancellationToken>())).ReturnsAsync(new DeleteMessageResponse()).Verifiable();

            amazonSQSClientFactoryMock.Setup(e => e.Create()).Returns(clientFake.Object);
            var sut = new QueueMessageManager(amazonSQSClientFactoryMock.Object, options);

            // Act
            await sut.DeleteMessageAsync(message, CancellationToken.None);

            // Assert
            clientFake.Verify();
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_DeleteMessageAsync_is_called_when_the_queue_doesnt_exsit_then_it_is_handled_correctly()
        {
            // Arrange
            var options = new HostOptions();
            var amazonSQSClientFactoryMock = new Mock<IAmazonSQSClientFactory>(MockBehavior.Strict);
            var clientFake = new Mock<AmazonSQSClient>("TESTKEY", "TESTSECRET", Amazon.RegionEndpoint.EUCentral1); ;
            var message = new MessageTestDataBuilder().Build();
            clientFake.Setup(r => r.DeleteMessageAsync(options.QueueUrl, message.ReceiptHandle, It.IsAny<CancellationToken>())).ThrowsAsync(new AmazonSQSException("testfail") { ErrorCode = "NonExistentQueue" });

            amazonSQSClientFactoryMock.Setup(e => e.Create()).Returns(clientFake.Object);
            var sut = new QueueMessageManager(amazonSQSClientFactoryMock.Object, options);

            // Act
            await sut.DeleteMessageAsync(message, CancellationToken.None);

            // Assert
            clientFake.Verify();
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_DeleteMessageAsync_is_called_when_the_message_doesnt_exsit_then_it_is_handled_correctly()
        {
            // Arrange
            var options = new HostOptions();
            var amazonSQSClientFactoryMock = new Mock<IAmazonSQSClientFactory>(MockBehavior.Strict);
            var clientFake = new Mock<AmazonSQSClient>("TESTKEY", "TESTSECRET", Amazon.RegionEndpoint.EUCentral1); ;
            var message = new MessageTestDataBuilder().Build();
            clientFake.Setup(r => r.DeleteMessageAsync(options.QueueUrl, message.ReceiptHandle, It.IsAny<CancellationToken>())).ThrowsAsync(new AmazonSQSException("testfail") { StatusCode = System.Net.HttpStatusCode.NotFound });

            amazonSQSClientFactoryMock.Setup(e => e.Create()).Returns(clientFake.Object);
            var sut = new QueueMessageManager(amazonSQSClientFactoryMock.Object, options);

            // Act
            await sut.DeleteMessageAsync(message, CancellationToken.None);

            // Assert
            clientFake.Verify();
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_DeleteMessageAsync_is_called_when_a_different_exception_occures_then_it_is_thrown()
        {
            // Arrange
            var options = new HostOptions();
            var amazonSQSClientFactoryMock = new Mock<IAmazonSQSClientFactory>(MockBehavior.Strict);
            var clientFake = new Mock<AmazonSQSClient>("TESTKEY", "TESTSECRET", Amazon.RegionEndpoint.EUCentral1); ;
            var message = new MessageTestDataBuilder().Build();
            clientFake.Setup(r => r.DeleteMessageAsync(options.QueueUrl, message.ReceiptHandle, It.IsAny<CancellationToken>())).ThrowsAsync(new AmazonSQSException("testfail") { StatusCode = System.Net.HttpStatusCode.BadGateway });

            amazonSQSClientFactoryMock.Setup(e => e.Create()).Returns(clientFake.Object);
            var sut = new QueueMessageManager(amazonSQSClientFactoryMock.Object, options);

            // Act
            Func<Task> act = () => sut.DeleteMessageAsync(message, CancellationToken.None);

            // Assert
            await act.Should().ThrowExactlyAsync<AmazonSQSException>();
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_UpdateVisibilityTimeOutAsync_is_called_then_messages_are_retrieved_correctly()
        {
            // Arrange
            var options = new HostOptions();
            var amazonSQSClientFactoryMock = new Mock<IAmazonSQSClientFactory>(MockBehavior.Strict);
            var clientFake = new Mock<AmazonSQSClient>("TESTKEY", "TESTSECRET", Amazon.RegionEndpoint.EUCentral1); ;
            var message = new MessageTestDataBuilder().Build();

            clientFake.Setup(r => r.ChangeMessageVisibilityAsync(It.Is<ChangeMessageVisibilityRequest>(o => o.VisibilityTimeout == options.VisibilityTimeoutInSeconds), It.IsAny<CancellationToken>())).ReturnsAsync(new ChangeMessageVisibilityResponse()).Verifiable();

            amazonSQSClientFactoryMock.Setup(e => e.Create()).Returns(clientFake.Object);
            var sut = new QueueMessageManager(amazonSQSClientFactoryMock.Object, options);

            // Act
            var nextVisbileOn = await sut.UpdateVisibilityTimeOutAsync(message, CancellationToken.None);

            // Assert
            clientFake.Verify();
            nextVisbileOn.Should().BeCloseTo(DateTimeOffset.UtcNow.Add(TimeSpan.FromSeconds(options.VisibilityTimeoutInSeconds)), TimeSpan.FromMilliseconds(100));
        }

        [Fact]
        public async Task Given_a_QueueMessageManager_when_EnqueueMessageAsync_is_called_then_messages_are_retrieved_correctly()
        {
            // Arrange
            var options = new HostOptions();
            var amazonSQSClientFactoryMock = new Mock<IAmazonSQSClientFactory>(MockBehavior.Strict);
            var clientFake = new Mock<AmazonSQSClient>("TESTKEY", "TESTSECRET", Amazon.RegionEndpoint.EUCentral1); ;
            var message = new MessageTestDataBuilder().Build();

            clientFake.Setup(r => r.ChangeMessageVisibilityAsync(It.Is<ChangeMessageVisibilityRequest>(o => o.VisibilityTimeout == 0), It.IsAny<CancellationToken>())).ReturnsAsync(new ChangeMessageVisibilityResponse()).Verifiable();

            amazonSQSClientFactoryMock.Setup(e => e.Create()).Returns(clientFake.Object);
            var sut = new QueueMessageManager(amazonSQSClientFactoryMock.Object, options);

            // Act
            await sut.EnqueueMessageAsync(message, CancellationToken.None);

            // Assert
            clientFake.Verify();
        }
    }
}

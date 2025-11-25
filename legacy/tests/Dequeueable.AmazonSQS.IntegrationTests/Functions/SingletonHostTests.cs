using Amazon.SQS;
using Amazon.SQS.Model;
using Dequeueable.AmazonSQS.Factories;
using Dequeueable.AmazonSQS.IntegrationTests.Fixtures;
using Dequeueable.AmazonSQS.IntegrationTests.TestDataBuilders;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dequeueable.AmazonSQS.IntegrationTests.Functions
{
    public class SingletonHostTests : IClassFixture<LocalStackFixture>, IAsyncLifetime
    {
        private readonly LocalStackFixture _localStackFixture;
        private readonly AmazonSQSClient _client;
        private string _queueUrl = null!;

        public SingletonHostTests(LocalStackFixture localStackFixture)
        {
            _localStackFixture = localStackFixture;
            _client = new AmazonSQSClient("dummy", "dummy", new AmazonSQSConfig { ServiceURL = _localStackFixture.SQSURL });
        }

        public async Task InitializeAsync()
        {
            var res = await _client.CreateQueueAsync(new CreateQueueRequest { Attributes = new Dictionary<string, string> { { "FifoQueue", "true" }, { "ContentBasedDeduplication", "true" } }, QueueName = "testqueue.fifo" });
            _queueUrl = res.QueueUrl;
        }

        public Task DisposeAsync()
        {
            return _client.DeleteQueueAsync(_queueUrl);
        }

        [Fact]
        public async Task Given_a_host_running_as_a_singleton_job_when_a_Queue_has_two_messages_of_the_same_group_then_they_are_handled_correctly()
        {
            // Arrange
            var messages = new List<SendMessageBatchRequestEntry>
            {
                new("1", "body1"){MessageGroupId ="1"},
                new("2", "body2"){MessageGroupId ="1"}
            };
            var response = await _client.SendMessageBatchAsync(new SendMessageBatchRequest
            {
                QueueUrl = _queueUrl,
                Entries = messages
            });

            var options = new Configurations.HostOptions
            {
                VisibilityTimeoutInSeconds = 500,
                QueueUrl = _queueUrl
            };
            var factory = new JobHostFactory<TestFunction>(opt =>
            {
                opt.VisibilityTimeoutInSeconds = options.VisibilityTimeoutInSeconds;
                opt.QueueUrl = options.QueueUrl;
            }, runAsSingleton: true);

            var fakeServiceMock = new Mock<IFakeService>();
            var amazonSQSClientFactoryMock = new Mock<IAmazonSQSClientFactory>();
            amazonSQSClientFactoryMock.Setup(c => c.Create()).Returns(_client);

            factory.ConfigureTestServices(services =>
            {
                services.AddTransient(_ => fakeServiceMock.Object);
                services.AddTransient(_ => amazonSQSClientFactoryMock.Object);
            });

            // Act
            var host = factory.Build();
            await host.HandleAsync(CancellationToken.None);

            // Assert
            var queueResult = await _client.ReceiveMessageAsync(new ReceiveMessageRequest { MaxNumberOfMessages = 10, QueueUrl = _queueUrl });
            queueResult.Messages.Should().BeEmpty();

            foreach (var message in messages)
            {
                fakeServiceMock.Verify(f => f.Execute(It.Is<Models.Message>(m => m.Body.ToString() == message.MessageBody)), Times.Once());
            }
        }

        [Fact]
        public async Task Given_a_host_running_as_a_singleton_listener_when_a_Queue_has_two_messages_of_the_same_group_then_they_are_handled_correctly()
        {
            // Arrange
            var messages = new List<SendMessageBatchRequestEntry>
            {
                new("1", "body1"){MessageGroupId ="1"},
                new("2", "body2"){MessageGroupId ="1"}
            };
            var response = await _client.SendMessageBatchAsync(new SendMessageBatchRequest
            {
                QueueUrl = _queueUrl,
                Entries = messages
            });

            var options = new Configurations.ListenerHostOptions
            {
                VisibilityTimeoutInSeconds = 500,
                QueueUrl = _queueUrl
            };
            var factory = new ListenerHostFactory<TestFunction>(opt =>
            {
                opt.VisibilityTimeoutInSeconds = options.VisibilityTimeoutInSeconds;
                opt.MinimumPollingIntervalInMilliseconds = 1;
                opt.MaximumPollingIntervalInMilliseconds = 1000;
                opt.QueueUrl = options.QueueUrl;
            }, runAsSingleton: true);

            var fakeServiceMock = new Mock<IFakeService>();
            var amazonSQSClientFactoryMock = new Mock<IAmazonSQSClientFactory>();
            amazonSQSClientFactoryMock.Setup(c => c.Create()).Returns(_client);

            factory.ConfigureTestServices(services =>
            {
                services.AddTransient(_ => fakeServiceMock.Object);
                services.AddTransient(_ => amazonSQSClientFactoryMock.Object);
            });

            // Act
            var host = factory.Build();
            await host.HandleAsync(CancellationToken.None);

            // Assert
            var queueResult = await _client.ReceiveMessageAsync(new ReceiveMessageRequest { MaxNumberOfMessages = 10, QueueUrl = _queueUrl });
            queueResult.Messages.Should().BeEmpty();

            foreach (var message in messages)
            {
                fakeServiceMock.Verify(f => f.Execute(It.Is<Models.Message>(m => m.Body.ToString() == message.MessageBody)), Times.Once());
            }
        }
    }
}

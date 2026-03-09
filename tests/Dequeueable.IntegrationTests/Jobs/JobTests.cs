using Azure.Storage.Queues;
using Dequeueable.IntegrationTests.Fixtures;
using Dequeueable.IntegrationTests.TestDataBuilders;
using Dequeueable.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Dequeueable.IntegrationTests.Jobs
{
    public class JobTests : IClassFixture<AzuriteFixture>, IAsyncLifetime
    {
        private readonly QueueClientOptions _queueClientOptions = new() { MessageEncoding = QueueMessageEncoding.Base64 };
        private readonly AzuriteFixture _azuriteFixture;
        private readonly string _queueName;
        private readonly QueueClient _queueClient;

        public JobTests(AzuriteFixture azuriteFixture)
        {
            _azuriteFixture = azuriteFixture;
            _queueName = "jobqueue";
            _queueClient = new QueueClient(_azuriteFixture.ConnectionString, _queueName, _queueClientOptions);
        }

        public Task InitializeAsync() => _queueClient.CreateAsync();
        public Task DisposeAsync() => _queueClient.DeleteAsync();

        [Fact]
        public async Task Given_a_Queue_when_is_has_two_messages_then_only_one_is_handled_correctly()
        {
            // Arrange
            var fakeService = new FakeService();
            var factory = new JobHostFactory<TestJob>(opt =>
            {
                opt.ConnectionString = _azuriteFixture.ConnectionString;
                opt.QueueName = _queueName;
            });

            factory.ConfigureTestServices(services => services.AddTransient<IFakeService>(_ => fakeService));

            var messages = new[] { "message1", "message2" };
            foreach (var message in messages)
                await _queueClient.SendMessageAsync(message);

            // Act
            await factory.Build().ExecuteAsync(CancellationToken.None);

            // Assert
            var peekedMessages = await _queueClient.PeekMessagesAsync();
            Assert.Single(peekedMessages.Value);

            Assert.Single(fakeService.ExecutedMessages);
            var executedBody = fakeService.ExecutedMessages[0].Body.ToString();
            Assert.Contains(executedBody, messages);

            var remainingBody = peekedMessages.Value[0].Body.ToString();
            Assert.NotEqual(executedBody, remainingBody);
        }

        [Fact]
        public async Task Given_a_QueueMessage_with_DequeueCount_1_when_an_error_occurred_while_executing_the_function_and_the_MaxDequeueCount_is_not_yet_reached_then_the_message_is_enqueued_correctly()
        {
            // Arrange
            var fakeService = new FakeService(shouldThrow: true);
            var factory = new JobHostFactory<TestJob>(opt =>
            {
                opt.ConnectionString = _azuriteFixture.ConnectionString;
                opt.QueueName = _queueName;
                opt.MaxDequeueCount = 5;
            });

            factory.ConfigureTestServices(services => services.AddTransient<IFakeService>(_ => fakeService));

            var message = "message1";
            await _queueClient.SendMessageAsync(message);

            // Act
            await factory.Build().ExecuteAsync(CancellationToken.None);

            // Assert
            var peekedMessage = await _queueClient.PeekMessageAsync();
            Assert.NotNull(peekedMessage.Value);
            Assert.Equal(message, peekedMessage.Value.Body.ToString());
            Assert.Equal(1, peekedMessage.Value.DequeueCount);
        }

        [Fact]
        public async Task Given_a_QueueMessage_with_DequeueCount_1_when_an_error_occurred_while_executing_the_function_and_the_MaxDequeueCount_is_reached_then_the_message_is_moved_to_the_poison_queue()
        {
            // Arrange
            var poisonQueueSuffix = "poison";
            var fakeService = new FakeService(shouldThrow: true);
            var factory = new JobHostFactory<TestJob>(opt =>
            {
                opt.ConnectionString = _azuriteFixture.ConnectionString;
                opt.QueueName = _queueName;
                opt.MaxDequeueCount = 1;
                opt.PoisonQueueSuffix = poisonQueueSuffix;
            });

            factory.ConfigureTestServices(services => services.AddTransient<IFakeService>(_ => fakeService));

            var message = "message1";
            await _queueClient.SendMessageAsync(message);

            // Act
            await factory.Build().ExecuteAsync(CancellationToken.None);

            // Assert
            var peekedMessage = await _queueClient.PeekMessageAsync();
            Assert.Null(peekedMessage.Value);

            var poisonQueueClient = new QueueClient(_azuriteFixture.ConnectionString, $"{_queueName}-{poisonQueueSuffix}", _queueClientOptions);
            var peekedPoisonMessage = await poisonQueueClient.PeekMessageAsync();
            Assert.NotNull(peekedPoisonMessage.Value);
            Assert.Equal(message, peekedPoisonMessage.Value.Body.ToString());
        }
    }
}
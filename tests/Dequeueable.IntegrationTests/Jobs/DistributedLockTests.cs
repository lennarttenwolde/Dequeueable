using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Dequeueable.Configurations;
using Dequeueable.IntegrationTests.Fixtures;
using Dequeueable.IntegrationTests.TestDataBuilders;
using Dequeueable.Services.DistributedLock;
using Microsoft.Extensions.DependencyInjection;

namespace Dequeueable.IntegrationTests.Jobs
{
    public class DistributedLockTests : IClassFixture<AzuriteFixture>, IAsyncLifetime
    {
        private readonly QueueClientOptions _queueClientOptions = new() { MessageEncoding = QueueMessageEncoding.Base64 };
        private readonly AzuriteFixture _azuriteFixture;
        private readonly string _queueName;
        private readonly QueueClient _queueClient;
        private readonly string _containerName = "joblock";
        private readonly string _scope = "Id";

        public DistributedLockTests(AzuriteFixture azuriteFixture)
        {
            _azuriteFixture = azuriteFixture;
            _queueName = "singletonqueue";
            _queueClient = new QueueClient(_azuriteFixture.ConnectionString, _queueName, _queueClientOptions);
        }

        public Task InitializeAsync() => _queueClient.CreateAsync();
        public Task DisposeAsync() => _queueClient.DeleteAsync();

        [Fact]
        public async Task Given_two_JobInstances_run_as_distributed_lock_when_a_queue_has_two_messages_then_both_are_handled_correctly()
        {
            // Arrange
            var fakeService = new FakeService();
            var factory = new JobHostFactory<TestJob>(opt =>
            {
                opt.ConnectionString = _azuriteFixture.ConnectionString;
                opt.QueueName = _queueName;

            }, opt =>
            {
                opt.ContainerName = _containerName;
                opt.Scope = _scope;
            });

            factory.ConfigureTestServices(services => services.AddTransient<IFakeService>(_ => fakeService));

            var messages = new[] { new { Id = "1" }, new { Id = "1" } };
            foreach (var message in messages)
                await _queueClient.SendMessageAsync(BinaryData.FromObjectAsJson(message));

            // Act
            var host = factory.Build();

            await Task.WhenAll(
                host.ExecuteAsync(CancellationToken.None),
                host.ExecuteAsync(CancellationToken.None));

            // Assert
            Assert.Equal(messages.Length, fakeService.ExecutedMessages.Count);

            var peekedMessage = await _queueClient.PeekMessageAsync();
            Assert.Null(peekedMessage.Value);

            var blobClient = new BlobContainerClient(_azuriteFixture.ConnectionString, _containerName).GetBlobClient("1");
            Assert.True((await blobClient.ExistsAsync()).Value);
        }
    }
}
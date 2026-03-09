using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Dequeueable.IntegrationTests.TestDataBuilders;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Azurite;

namespace Dequeueable.IntegrationTests.Jobs
{
    public class DistributedLockTests : IAsyncLifetime
    {
        private readonly QueueClientOptions _queueClientOptions = new() { MessageEncoding = QueueMessageEncoding.Base64 };
        private readonly AzuriteContainer _azuriteContainer;
        private readonly string _queueName = "singletonqueue";
        private QueueClient _queueClient = null!;
        private readonly string _containerName = "joblock";
        private readonly string _scope = "Id";

        public DistributedLockTests()
        {
            _azuriteContainer = new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:3.35.0")
                .WithAutoRemove(true)
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _azuriteContainer.StartAsync();
            _queueClient = new QueueClient(_azuriteContainer.GetConnectionString(), _queueName, _queueClientOptions);
            await _queueClient.CreateAsync();

        }
        public async Task DisposeAsync()
        {
            await _queueClient.DeleteAsync();
            await _azuriteContainer.DisposeAsync();
        }

        [Fact]
        public async Task Given_two_JobInstances_run_as_distributed_lock_when_a_queue_has_two_messages_then_both_are_handled_correctly()
        {
            // Arrange
            var fakeService = new FakeService();
            var factory = new JobHostFactory<TestJob>(opt =>
            {
                opt.ConnectionString = _azuriteContainer.GetConnectionString();
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

            var blobClient = new BlobContainerClient(_azuriteContainer.GetConnectionString(), _containerName).GetBlobClient("1");
            Assert.True((await blobClient.ExistsAsync()).Value);
        }
    }
}
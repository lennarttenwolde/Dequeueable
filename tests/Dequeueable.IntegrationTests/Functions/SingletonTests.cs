using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Dequeueable.IntegrationTests.Fixtures;
using Dequeueable.IntegrationTests.TestDataBuilders;
using Dequeueable.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dequeueable.IntegrationTests.Functions
{
    public class SingletonTests : IClassFixture<AzuriteFixture>, IAsyncLifetime
    {
        private readonly QueueClientOptions _queueClientOptions = new() { MessageEncoding = QueueMessageEncoding.Base64 };
        private readonly AzuriteFixture _azuriteFixture;
        private readonly string _queueName;
        private readonly QueueClient _queueClient;

        public SingletonTests(AzuriteFixture azuriteFixture)
        {
            _azuriteFixture = azuriteFixture;
            _queueName = "singletonqueue";
            _queueClient = new QueueClient(_azuriteFixture.ConnectionString, _queueName, _queueClientOptions);
        }

        public Task InitializeAsync()
        {
            return _queueClient.CreateAsync();
        }

        public Task DisposeAsync()
        {
            return _queueClient.DeleteAsync();
        }

        [Fact]
        public async Task Given_two_JobInstances_run_as_distributed_lock_when_a_queue_has_two_messages_then_only_one_is_handled_correctly()
        {
            // Arrange
            var scope = "Id";
            var containerName = "joblock";
            var factory = new JobHostFactory<TestFunction>(opt =>
            {
                opt.ConnectionString = _azuriteFixture.ConnectionString;
                opt.QueueName = _queueName;
                opt.MaxDequeueCount = 5;
            }, opt =>
            {
                opt.ContainerName = containerName;
                opt.Scope = scope;
            });

            var fakeServiceMock = new Mock<IFakeService>();

            factory.ConfigureTestServices(services =>
            {
                services.AddTransient(_ => fakeServiceMock.Object);
            });

            var blobContainerClient = new BlobContainerClient(_azuriteFixture.ConnectionString, containerName);

            var messages = new[] { new { Id = "1" }, new { Id = "1" } };
            foreach (var message in messages)
            {
                await _queueClient.SendMessageAsync(BinaryData.FromObjectAsJson(message));
            }

            // Act
            var host = factory.Build();

            await Task.WhenAll(
                host.ExecuteAsync(CancellationToken.None),
                host.ExecuteAsync(CancellationToken.None));

            // Assert
            fakeServiceMock.Verify(f => f.Execute(It.IsAny<Message>()), Times.Exactly(messages.Length));
            var peekedMessage = await _queueClient.PeekMessageAsync();
            peekedMessage.Value.Should().BeNull();

            var blobclient = blobContainerClient.GetBlobClient("1");
            (await blobclient.ExistsAsync()).Value.Should().BeTrue();
        }
    }
}

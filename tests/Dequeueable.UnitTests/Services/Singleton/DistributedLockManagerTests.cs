using Azure.Storage.Blobs;
using Dequeueable.Configurations;
using Dequeueable.Factories;
using Dequeueable.Services.DistributedLock;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Dequeueable.UnitTests.Services.Singleton
{
    public class DistributedLockManagerTests
    {
        [Fact]
        public async Task Given_a_LockManager_when_AquireLockAsync_is_called_and_the_lock_is_acquired_then_the_leaseId_is_returned()
        {
            // Arrange
            var leaseId = "someId";
            var fileName = "someName";
            var options = new HostOptions { ConnectionString = "some string" };
            var loggerMock = new Mock<ILogger<DistributedLockManager>>(MockBehavior.Strict);

            var blobClientProviderMock = new Mock<IBlobClientProvider>(MockBehavior.Strict);
            var blobLeaseManagerFactoryMock = new Mock<IBlobLeaseManagerFactory>(MockBehavior.Strict);
            var blobLeaseManagerMock = new Mock<IBlobLeaseManager>(MockBehavior.Strict);
            var distributedLockOptions = new DistributedLockOptions();
            var singletonHostOptionsMock = new Mock<IOptions<DistributedLockOptions>>();
            var blobClientFake = new Mock<BlobClient>();

            singletonHostOptionsMock.Setup(o => o.Value).Returns(distributedLockOptions);
            blobClientProviderMock.Setup(c => c.GetClient(fileName)).Returns(blobClientFake.Object);
            blobLeaseManagerFactoryMock.Setup(f => f.Create(blobClientFake.Object, distributedLockOptions, loggerMock.Object)).Returns(blobLeaseManagerMock.Object);
            loggerMock.Setup(
                x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Lock with Id '{leaseId}' acquired for '{fileName}'")),
                null,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)))
                .Verifiable();
            blobLeaseManagerMock.Setup(m => m.AcquireAsync(CancellationToken.None)).ReturnsAsync(leaseId);

            var sut = new DistributedLockManager(loggerMock.Object, blobClientProviderMock.Object, blobLeaseManagerFactoryMock.Object, singletonHostOptionsMock.Object);

            // Act
            var result = await sut.AquireLockAsync(fileName, CancellationToken.None);

            // Assert
            result.Should().Be(leaseId);
        }

        [Fact]
        public async Task Given_a_LockManager_when_AquireLockAsync_is_called_and_the_lock_cannot_be_acquired_at_first_then_it_is_retried_correctly()
        {
            // Arrange
            var leaseId = "someId";
            var fileName = "someName";
            var options = new HostOptions { ConnectionString = "some string" };
            var loggerMock = new Mock<ILogger<DistributedLockManager>>(MockBehavior.Strict);

            var blobClientProviderMock = new Mock<IBlobClientProvider>(MockBehavior.Strict);
            var blobLeaseManagerFactoryMock = new Mock<IBlobLeaseManagerFactory>(MockBehavior.Strict);
            var blobLeaseManagerMock = new Mock<IBlobLeaseManager>(MockBehavior.Strict);
            var distributedLockOptions = new DistributedLockOptions { MaxRetries = 5, MinimumPollingIntervalInSeconds = 1, MaximumPollingIntervalInSeconds = 1 };
            var singletonHostOptionsMock = new Mock<IOptions<DistributedLockOptions>>();
            var blobClientFake = new Mock<BlobClient>();

            singletonHostOptionsMock.Setup(o => o.Value).Returns(distributedLockOptions);
            blobClientProviderMock.Setup(c => c.GetClient(fileName)).Returns(blobClientFake.Object);
            blobLeaseManagerFactoryMock.Setup(f => f.Create(blobClientFake.Object, distributedLockOptions, loggerMock.Object)).Returns(blobLeaseManagerMock.Object);
            loggerMock.Setup(
                x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Lock with Id '{leaseId}' acquired for '{fileName}'")),
                null,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)))
                .Verifiable();
            blobLeaseManagerMock.SetupSequence(m => m.AcquireAsync(CancellationToken.None))
                .ReturnsAsync((string?)null)
                .ReturnsAsync(leaseId);

            var sut = new DistributedLockManager(loggerMock.Object, blobClientProviderMock.Object, blobLeaseManagerFactoryMock.Object, singletonHostOptionsMock.Object);

            // Act
            var result = await sut.AquireLockAsync(fileName, CancellationToken.None);

            // Assert
            result.Should().Be(leaseId);
        }

        [Fact]
        public async Task Given_a_LockManager_when_AquireLockAsync_is_called_and_the_lock_cannot_be_acquired_and_the_MaxRetries_is_reached_then_a_SingletonException_is_thrown()
        {
            // Arrange
            var fileName = "someName";
            var options = new HostOptions { ConnectionString = "some string" };
            var loggerMock = new Mock<ILogger<DistributedLockManager>>(MockBehavior.Strict);

            var blobClientProviderMock = new Mock<IBlobClientProvider>(MockBehavior.Strict);
            var blobLeaseManagerFactoryMock = new Mock<IBlobLeaseManagerFactory>(MockBehavior.Strict);
            var blobLeaseManagerMock = new Mock<IBlobLeaseManager>(MockBehavior.Strict);
            var distributedLockOptions = new DistributedLockOptions { MaxRetries = 1, MinimumPollingIntervalInSeconds = 1, MaximumPollingIntervalInSeconds = 1 };
            var singletonHostOptionsMock = new Mock<IOptions<DistributedLockOptions>>();
            var blobClientFake = new Mock<BlobClient>();

            singletonHostOptionsMock.Setup(o => o.Value).Returns(distributedLockOptions);
            blobClientProviderMock.Setup(c => c.GetClient(fileName)).Returns(blobClientFake.Object);
            blobLeaseManagerFactoryMock.Setup(f => f.Create(blobClientFake.Object, distributedLockOptions, loggerMock.Object)).Returns(blobLeaseManagerMock.Object);
            blobLeaseManagerMock.SetupSequence(m => m.AcquireAsync(CancellationToken.None))
                .ReturnsAsync((string?)null)
                .ReturnsAsync((string?)null);

            var sut = new DistributedLockManager(loggerMock.Object, blobClientProviderMock.Object, blobLeaseManagerFactoryMock.Object, singletonHostOptionsMock.Object);

            // Act
            Func<Task> act = () => sut.AquireLockAsync(fileName, CancellationToken.None);

            // Assert
            await act.Should().ThrowExactlyAsync<DistributedLockException>().WithMessage($"Unable to acquire lock, max retries of '{distributedLockOptions.MaxRetries}' reached");
        }
    }
}

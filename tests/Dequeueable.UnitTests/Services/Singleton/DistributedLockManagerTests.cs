using Dequeueable.Configurations;
using Dequeueable.Factories;
using Dequeueable.Services.DistributedLock;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Dequeueable.UnitTests.Services.Singleton
{
    public class DistributedLockManagerTests
    {
        private readonly string _fileName = "someName";
        private readonly string _leaseId = "someId";

        private (DistributedLockManager sut, FakeLogger<DistributedLockManager> logger, IBlobLeaseManager leaseManager) CreateSut(DistributedLockOptions? options = null)
        {
            var logger = new FakeLogger<DistributedLockManager>();
            var leaseManager = Substitute.For<IBlobLeaseManager>();
            var factory = Substitute.For<IBlobLeaseManagerFactory>();
            factory.Create(_fileName).Returns(leaseManager);

            var sut = new DistributedLockManager(logger, factory, Options.Create(options ?? new DistributedLockOptions()));
            return (sut, logger, leaseManager);
        }

        [Fact]
        public async Task Given_a_LockManager_when_AquireLockAsync_is_called_and_the_lock_is_acquired_then_the_leaseId_is_returned()
        {
            // Arrange
            var (sut, logger, leaseManager) = CreateSut();
            leaseManager.AcquireAsync(Arg.Any<CancellationToken>()).Returns(_leaseId);

            // Act
            var result = await sut.AquireLockAsync(_fileName, CancellationToken.None);

            // Assert
            Assert.Equal(_leaseId, result);
            Assert.Contains(logger.Collector.GetSnapshot(), e => e.Level == LogLevel.Information && e.Message.Contains($"Lock with Id '{_leaseId}' acquired for '{_fileName}'", StringComparison.Ordinal));
        }

        [Fact]
        public async Task Given_a_LockManager_when_AquireLockAsync_is_called_and_the_lock_cannot_be_acquired_at_first_then_it_is_retried_correctly()
        {
            // Arrange
            var (sut, _, leaseManager) = CreateSut(new DistributedLockOptions
            {
                MaxRetries = 5,
                MinimumPollingIntervalInSeconds = 0,
                MaximumPollingIntervalInSeconds = 1
            });

            leaseManager.AcquireAsync(Arg.Any<CancellationToken>()).Returns((string?)null, _leaseId);

            // Act
            var result = await sut.AquireLockAsync(_fileName, CancellationToken.None);

            // Assert
            Assert.Equal(_leaseId, result);
            await leaseManager.Received(2).AcquireAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Given_a_LockManager_when_AquireLockAsync_is_called_and_the_lock_cannot_be_acquired_and_the_MaxRetries_is_reached_then_a_DistributedLockException_is_thrown()
        {
            // Arrange
            var (sut, _, leaseManager) = CreateSut(new DistributedLockOptions
            {
                MaxRetries = 1,
                MinimumPollingIntervalInSeconds = 0,
                MaximumPollingIntervalInSeconds = 1
            });

            leaseManager.AcquireAsync(Arg.Any<CancellationToken>()).Returns((string?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<DistributedLockException>(() => sut.AquireLockAsync(_fileName, CancellationToken.None));
            Assert.Equal("Unable to acquire lock, max retries of '1' reached", ex.Message);
        }

        [Fact]
        public async Task Given_a_LockManager_when_AquireLockAsync_is_called_and_cancellation_is_requested_then_a_DistributedLockException_is_thrown()
        {
            // Arrange
            var (sut, _, leaseManager) = CreateSut(new DistributedLockOptions
            {
                MaxRetries = 10,
                MinimumPollingIntervalInSeconds = 0,
                MaximumPollingIntervalInSeconds = 1
            });

            leaseManager.AcquireAsync(Arg.Any<CancellationToken>()).Returns((string?)null);
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            // Act & Assert
            await Assert.ThrowsAsync<DistributedLockException>(() => sut.AquireLockAsync(_fileName, cts.Token));
        }

        [Fact]
        public async Task Given_a_LockManager_when_RenewLockAsync_is_called_then_the_lock_is_renewed_and_logged()
        {
            // Arrange
            var (sut, logger, leaseManager) = CreateSut();
            var nextVisible = DateTimeOffset.UtcNow.AddMinutes(1);
            leaseManager.RenewAsync(_leaseId, Arg.Any<CancellationToken>()).Returns(nextVisible);

            // Act
            var result = await sut.RenewLockAsync(_leaseId, _fileName, CancellationToken.None);

            // Assert
            Assert.Equal(nextVisible, result);
            Assert.Contains(logger.Collector.GetSnapshot(), e => e.Level == LogLevel.Information && e.Message.Contains($"Lock with Id '{_leaseId}' renewed", StringComparison.Ordinal));
        }

        [Fact]
        public async Task Given_a_LockManager_when_ReleaseLockAsync_is_called_then_the_lock_is_released()
        {
            // Arrange
            var (sut, _, leaseManager) = CreateSut();
            leaseManager.ReleaseAsync(_leaseId, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

            // Act
            await sut.ReleaseLockAsync(_leaseId, _fileName, CancellationToken.None);

            // Assert
            await leaseManager.Received(1).ReleaseAsync(_leaseId, Arg.Any<CancellationToken>());
        }
    }
}
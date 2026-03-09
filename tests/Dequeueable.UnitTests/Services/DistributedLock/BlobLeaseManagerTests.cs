using Azure;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Dequeueable.Configurations;
using Dequeueable.Services.DistributedLock;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dequeueable.UnitTests.Services.DistributedLock
{
    public class BlobLeaseManagerTests
    {
        private readonly string _blobName = "some-blob";
        private readonly string _leaseId = "someId";
        private readonly DistributedLockOptions _options = new();

        private (BlobLeaseManager sut, FakeLogger logger) CreateSut(out BlobSubstitutes blobs)
        {
            var logger = new FakeLogger();
            var blobClient = Substitute.For<Azure.Storage.Blobs.BlobClient>();
            var leaseClient = Substitute.For<BlobLeaseClient>();
            var blobClientProvider = Substitute.For<IBlobClientProvider>();

            blobClient.GetBlobLeaseClient(Arg.Any<string>()).Returns(leaseClient);
            blobClientProvider.GetClient(_blobName).Returns(blobClient);

            blobs = new BlobSubstitutes(blobClient, leaseClient, blobClientProvider);
            return (new BlobLeaseManager(_blobName, blobClientProvider, _options, logger), logger);
        }

        [Theory]
        [InlineData(LeaseState.Available)]
        [InlineData(LeaseState.Expired)]
        [InlineData(LeaseState.Broken)]
        public async Task Given_a_BlobLeaseManager_when_AcquireLease_is_called_for_a_blob_that_exist_and_it_is_available_then_the_lease_is_acquired_correctly(LeaseState leaseState)
        {
            // Arrange
            var (sut, _) = CreateSut(out var blobs);
            var blobProperties = BlobsModelFactory.BlobProperties(leaseState: leaseState);
            var blobLease = BlobsModelFactory.BlobLease(new ETag(), DateTimeOffset.Now, leaseId: _leaseId);

            var propertiesResponse = Substitute.For<Response<BlobProperties>>();
            propertiesResponse.Value.Returns(blobProperties);
            var leaseResponse = Substitute.For<Response<BlobLease>>();
            leaseResponse.Value.Returns(blobLease);

            blobs.BlobClient.GetPropertiesAsync(null, Arg.Any<CancellationToken>()).Returns(propertiesResponse);
            blobs.LeaseClient.AcquireAsync(TimeSpan.FromSeconds(60), null, Arg.Any<CancellationToken>()).Returns(leaseResponse);

            // Act
            var result = await sut.AcquireAsync(CancellationToken.None);

            // Assert
            Assert.Equal(_leaseId, result);
        }

        [Fact]
        public async Task Given_a_BlobLeaseManager_when_AcquireLease_is_called_for_a_blob_that_exist_and_it_is_Leased_then_the_lease_is_not_acquired()
        {
            // Arrange
            var (sut, _) = CreateSut(out var blobs);
            var blobProperties = BlobsModelFactory.BlobProperties(leaseState: LeaseState.Leased);

            var propertiesResponse = Substitute.For<Response<BlobProperties>>();
            propertiesResponse.Value.Returns(blobProperties);
            blobs.BlobClient.GetPropertiesAsync(null, Arg.Any<CancellationToken>()).Returns(propertiesResponse);

            // Act
            var result = await sut.AcquireAsync(CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Given_a_DistributedBlobLeaseManager_when_AcquireAsync_is_called_for_a_blob_that_does_not_exist_then_the_lease_is_acquired_correctly()
        {
            // Arrange
            var (sut, _) = CreateSut(out var blobs);
            var blobLease = BlobsModelFactory.BlobLease(new ETag(), DateTimeOffset.Now, leaseId: _leaseId);
            var leaseResponse = Substitute.For<Response<BlobLease>>();
            leaseResponse.Value.Returns(blobLease);

            blobs.BlobClient.GetPropertiesAsync(null, Arg.Any<CancellationToken>()).ThrowsAsync(new RequestFailedException(404, "not found"));
            blobs.BlobClient.UploadAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(Substitute.For<Response<BlobContentInfo>>());
            blobs.LeaseClient.AcquireAsync(TimeSpan.FromSeconds(60), null, Arg.Any<CancellationToken>()).Returns(leaseResponse);

            // Act
            var result = await sut.AcquireAsync(CancellationToken.None);

            // Assert
            Assert.Equal(_leaseId, result);
        }

        [Fact]
        public async Task Given_a_DistributedBlobLeaseManager_when_AcquireAsync_is_called_for_a_blob_and_container_that_does_not_exist_then_the_lease_is_acquired_correctly()
        {
            // Arrange
            var (sut, _) = CreateSut(out var blobs);
            var blobLease = BlobsModelFactory.BlobLease(new ETag(), DateTimeOffset.Now, leaseId: _leaseId);
            var leaseResponse = Substitute.For<Response<BlobLease>>();
            leaseResponse.Value.Returns(blobLease);
            var containerClient = Substitute.For<Azure.Storage.Blobs.BlobContainerClient>();

            blobs.BlobClient.GetPropertiesAsync(null, Arg.Any<CancellationToken>()).ThrowsAsync(new RequestFailedException(404, "blob not found"));
            blobs.BlobClient.UploadAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
                .Returns(_ => throw new RequestFailedException(404, "container not found"), _ => Substitute.For<Response<BlobContentInfo>>());
            blobs.BlobClient.GetParentBlobContainerClient().Returns(containerClient);
            containerClient.CreateAsync(Arg.Any<PublicAccessType>(), null, null, Arg.Any<CancellationToken>())
                .Returns(Substitute.For<Response<BlobContainerInfo>>());
            blobs.LeaseClient.AcquireAsync(TimeSpan.FromSeconds(60), null, Arg.Any<CancellationToken>()).Returns(leaseResponse);

            // Act
            var result = await sut.AcquireAsync(CancellationToken.None);

            // Assert
            Assert.Equal(_leaseId, result);
        }

        [Fact]
        public async Task Given_a_DistributedBlobLeaseManager_when_AcquireAsync_is_called_and_the_created_container_already_exists_then_the_lease_is_acquired_correctly()
        {
            // Arrange
            var (sut, _) = CreateSut(out var blobs);
            var blobLease = BlobsModelFactory.BlobLease(new ETag(), DateTimeOffset.Now, leaseId: _leaseId);
            var leaseResponse = Substitute.For<Response<BlobLease>>();
            leaseResponse.Value.Returns(blobLease);
            var containerClient = Substitute.For<Azure.Storage.Blobs.BlobContainerClient>();

            blobs.BlobClient.GetPropertiesAsync(null, Arg.Any<CancellationToken>()).ThrowsAsync(new RequestFailedException(404, "blob not found"));
            blobs.BlobClient.UploadAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
                .Returns(_ => throw new RequestFailedException(404, "container not found"), _ => Substitute.For<Response<BlobContentInfo>>());
            blobs.BlobClient.GetParentBlobContainerClient().Returns(containerClient);
            containerClient.CreateAsync(Arg.Any<PublicAccessType>(), null, null, Arg.Any<CancellationToken>())
                .ThrowsAsync(new RequestFailedException(409, "container already exists"));
            blobs.LeaseClient.AcquireAsync(TimeSpan.FromSeconds(60), null, Arg.Any<CancellationToken>()).Returns(leaseResponse);

            // Act
            var result = await sut.AcquireAsync(CancellationToken.None);

            // Assert
            Assert.Equal(_leaseId, result);
        }

        [Theory]
        [InlineData(409)]
        [InlineData(412)]
        public async Task Given_a_DistributedBlobLeaseManager_when_AcquireAsync_is_called_for_a_blob_that_does_not_exist_and_is_concurrently_leased_and_exceptions_occurres_then_it_is_handled_correctly(int statusCode)
        {
            // Arrange
            var (sut, _) = CreateSut(out var blobs);
            var blobLease = BlobsModelFactory.BlobLease(new ETag(), DateTimeOffset.Now, leaseId: _leaseId);
            var leaseResponse = Substitute.For<Response<BlobLease>>();
            leaseResponse.Value.Returns(blobLease);

            blobs.BlobClient.GetPropertiesAsync(null, Arg.Any<CancellationToken>()).ThrowsAsync(new RequestFailedException(404, "blob not found"));
            blobs.BlobClient.UploadAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
                .Returns(_ => throw new RequestFailedException(statusCode, "conflict"), _ => Substitute.For<Response<BlobContentInfo>>());
            blobs.LeaseClient.AcquireAsync(TimeSpan.FromSeconds(60), null, Arg.Any<CancellationToken>()).Returns(leaseResponse);

            // Act
            var result = await sut.AcquireAsync(CancellationToken.None);

            // Assert
            Assert.Equal(_leaseId, result);
        }

        [Fact]
        public async Task Given_a_BlobLeaseManager_when_RenewAsync_is_called_for_a_blob_that_is_Leased_then_the_lease_is_renewed()
        {
            // Arrange
            var leaseDuration = TimeSpan.FromSeconds(60);
            var (sut, _) = CreateSut(out var blobs);
            var blobProperties = BlobsModelFactory.BlobProperties(leaseState: LeaseState.Leased);

            var propertiesResponse = Substitute.For<Response<BlobProperties>>();
            propertiesResponse.Value.Returns(blobProperties);
            blobs.BlobClient.GetPropertiesAsync(null, Arg.Any<CancellationToken>()).Returns(propertiesResponse);
            blobs.LeaseClient.RenewAsync(null, Arg.Any<CancellationToken>()).Returns(Substitute.For<Response<BlobLease>>());

            // Act
            var nextTimeout = await sut.RenewAsync(_leaseId, CancellationToken.None);

            // Assert
            Assert.True(nextTimeout >= DateTimeOffset.UtcNow.Add(leaseDuration).AddSeconds(-1));
            Assert.True(nextTimeout <= DateTimeOffset.UtcNow.Add(leaseDuration).AddSeconds(1));
        }

        [Fact]
        public async Task Given_a_BlobLeaseManager_when_RenewAsync_is_called_for_a_blob_that_is_NOT_Leased_then_a_DistributedLockException_is_thrown()
        {
            // Arrange
            var (sut, _) = CreateSut(out var blobs);
            var blobProperties = BlobsModelFactory.BlobProperties(leaseState: LeaseState.Broken);

            var propertiesResponse = Substitute.For<Response<BlobProperties>>();
            propertiesResponse.Value.Returns(blobProperties);
            blobs.BlobClient.GetPropertiesAsync(null, Arg.Any<CancellationToken>()).Returns(propertiesResponse);
            blobs.BlobClient.Name.Returns(_blobName);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<DistributedLockException>(() => sut.RenewAsync(_leaseId, CancellationToken.None));
            Assert.Equal($"Unable to renew the lock for {_blobName} because the lease is not active anymore", ex.Message);
        }

        [Fact]
        public async Task Given_a_BlobLeaseManager_when_RenewAsync_is_called_and_a_RequestFailedException_is_thrown_then_it_is_logged_and_rethrown_correctly()
        {
            // Arrange
            var (sut, logger) = CreateSut(out var blobs);
            var blobProperties = BlobsModelFactory.BlobProperties(leaseState: LeaseState.Leased);

            var propertiesResponse = Substitute.For<Response<BlobProperties>>();
            propertiesResponse.Value.Returns(blobProperties);
            blobs.BlobClient.GetPropertiesAsync(null, Arg.Any<CancellationToken>()).Returns(propertiesResponse);
            blobs.BlobClient.Name.Returns(_blobName);
            blobs.LeaseClient.RenewAsync(null, Arg.Any<CancellationToken>()).ThrowsAsync(new RequestFailedException(409, "some conflict"));

            // Act & Assert
            await Assert.ThrowsAsync<RequestFailedException>(() => sut.RenewAsync(_leaseId, CancellationToken.None));
            Assert.Contains(logger.Collector.GetSnapshot(), e => e.Level == LogLevel.Error && e.Message.Contains($"An error occurred while acquiring the lease for blob '{_blobName}'", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Given_a_BlobLeaseManager_when_ReleaseAsync_is_called_then_the_lease_is_released()
        {
            // Arrange
            var (sut, _) = CreateSut(out var blobs);
            blobs.LeaseClient.ReleaseAsync(null, Arg.Any<CancellationToken>()).Returns(Substitute.For<Response<ReleasedObjectInfo>>());

            // Act
            await sut.ReleaseAsync(_leaseId, CancellationToken.None);

            // Assert
            await blobs.LeaseClient.Received(1).ReleaseAsync(null, Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData(404)]
        [InlineData(409)]
        public async Task Given_a_BlobLeaseManager_when_ReleaseAsync_is_called_and_the_blob_does_not_exists_or_is_leased_by_somebody_else_then_the_exception_is_handled(int statusCode)
        {
            // Arrange
            var (sut, _) = CreateSut(out var blobs);
            blobs.LeaseClient.ReleaseAsync(null, Arg.Any<CancellationToken>()).ThrowsAsync(new RequestFailedException(statusCode, "some message"));

            // Act & Assert
            await sut.ReleaseAsync(_leaseId, CancellationToken.None); // should not throw
        }

        [Fact]
        public async Task Given_a_BlobLeaseManager_when_ReleaseAsync_is_called_and_a_server_error_occurs_then_the_exception_is_thrown()
        {
            // Arrange
            var (sut, _) = CreateSut(out var blobs);
            blobs.LeaseClient.ReleaseAsync(null, Arg.Any<CancellationToken>()).ThrowsAsync(new RequestFailedException(500, "server error"));

            // Act & Assert
            await Assert.ThrowsAsync<RequestFailedException>(() => sut.ReleaseAsync(_leaseId, CancellationToken.None));
        }
    }

    internal sealed record BlobSubstitutes(
        Azure.Storage.Blobs.BlobClient BlobClient,
        BlobLeaseClient LeaseClient,
        IBlobClientProvider BlobClientProvider);
}
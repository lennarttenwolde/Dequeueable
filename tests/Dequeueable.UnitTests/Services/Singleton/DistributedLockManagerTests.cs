using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Dequeueable.AzureQueueStorage.Configurations;
using Dequeueable.AzureQueueStorage.Services.Singleton;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace Dequeueable.AzureQueueStorage.UnitTests.Services.Singleton
{
    public class DistributedLockManagerTests
    {

        [Theory]
        [InlineData(LeaseState.Available)]
        [InlineData(LeaseState.Expired)]
        [InlineData(LeaseState.Broken)]
        public async Task Given_a_LockManager_when_AcquireLease_is_called_for_a_blob_that_exist_and_it_is_available_then_the_lease_is_acquired_correctly(LeaseState leaseState)
        {
            // Arrange
            var leaseId = "someId";
            var blobPropertiesFake = BlobsModelFactory.BlobProperties(leaseState: leaseState);

            var blobLeaseFake = BlobsModelFactory.BlobLease(new ETag(), DateTimeOffset.Now, leaseId: leaseId);
            var blobPropertiesResponseFake = new Mock<Response<BlobProperties>>(MockBehavior.Strict);
            var blobClientFake = new Mock<BlobClient>(MockBehavior.Strict);
            var blobLeaseClientFake = new Mock<BlobLeaseClient>(MockBehavior.Strict);
            var blobLeaseResponseFake = new Mock<Response<BlobLease>>(MockBehavior.Strict);
            var loggerMock = new Mock<ILogger>();
            var options = new SingletonHostOptions();

            blobPropertiesResponseFake.SetupGet(p => p.Value).Returns(blobPropertiesFake);
            blobLeaseResponseFake.SetupGet(p => p.Value).Returns(blobLeaseFake);
            blobLeaseClientFake.Setup(b => b.AcquireAsync(TimeSpan.FromSeconds(60), null, It.IsAny<CancellationToken>())).ReturnsAsync(blobLeaseResponseFake.Object);

            blobClientFake.Protected().Setup<BlobLeaseClient>("GetBlobLeaseClientCore", ItExpr.IsAny<string>())
                .Returns<string>((leaseId) => blobLeaseClientFake.Object);
            blobClientFake.Setup(b => b.GetPropertiesAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(blobPropertiesResponseFake.Object);

            var sut = new DistributedLockManager(blobClientFake.Object, options, loggerMock.Object);

            // Act
            var result = await sut.AcquireAsync(CancellationToken.None);

            // Assert
            result.Should().Be(leaseId);
        }

        [Fact]
        public async Task Given_a_LockManager_when_AcquireLease_is_called_for_a_blob_that_exist_and_it_is_Leased_then_the_lease_is_not_acquired()
        {
            // Arrange
            var leaseId = "someId";
            var blobPropertiesFake = BlobsModelFactory.BlobProperties(leaseState: LeaseState.Leased);

            var blobLeaseFake = BlobsModelFactory.BlobLease(new ETag(), DateTimeOffset.Now, leaseId: leaseId);
            var blobPropertiesResponseFake = new Mock<Response<BlobProperties>>(MockBehavior.Strict);
            var blobClientFake = new Mock<BlobClient>(MockBehavior.Strict);
            var blobLeaseClientFake = new Mock<BlobLeaseClient>(MockBehavior.Strict);
            var blobLeaseResponseFake = new Mock<Response<BlobLease>>(MockBehavior.Strict);
            var options = new SingletonHostOptions();
            var loggerMock = new Mock<ILogger>();

            blobPropertiesResponseFake.SetupGet(p => p.Value).Returns(blobPropertiesFake);
            blobLeaseResponseFake.SetupGet(p => p.Value).Returns(blobLeaseFake);
            blobLeaseClientFake.Setup(b => b.AcquireAsync(TimeSpan.FromSeconds(60), null, It.IsAny<CancellationToken>())).ReturnsAsync(blobLeaseResponseFake.Object);

            blobClientFake.Protected().Setup<BlobLeaseClient>("GetBlobLeaseClientCore", ItExpr.IsAny<string>())
                .Returns<string>((leaseId) => blobLeaseClientFake.Object);
            blobClientFake.Setup(b => b.GetPropertiesAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(blobPropertiesResponseFake.Object);

            var sut = new DistributedLockManager(blobClientFake.Object, options, loggerMock.Object);

            // Act
            var result = await sut.AcquireAsync(CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Given_a_DistributedLockManager_when_AcquireAsync_is_called_for_a_blob_that_does_not_exist_then_the_lease_is_acquired_correctly()
        {
            // Arrange
            var leaseId = "someId";
            var blobPropertiesFake = BlobsModelFactory.BlobProperties(leaseState: LeaseState.Available);
            var blobLeaseFake = BlobsModelFactory.BlobLease(new ETag(), DateTimeOffset.Now, leaseId: leaseId);
            var blobClientFake = new Mock<BlobClient>(MockBehavior.Strict);
            var blobLeaseClientFake = new Mock<BlobLeaseClient>(MockBehavior.Strict);
            var blobLeaseResponseFake = new Mock<Response<BlobLease>>(MockBehavior.Strict);
            var options = new SingletonHostOptions();
            var loggerMock = new Mock<ILogger>();

            blobLeaseResponseFake.SetupGet(p => p.Value).Returns(blobLeaseFake);
            blobLeaseClientFake.Setup(b => b.AcquireAsync(TimeSpan.FromSeconds(60), null, It.IsAny<CancellationToken>())).ReturnsAsync(blobLeaseResponseFake.Object);

            blobClientFake.Protected().Setup<BlobLeaseClient>("GetBlobLeaseClientCore", ItExpr.IsAny<string>())
                .Returns<string>((leaseId) => blobLeaseClientFake.Object);
            blobClientFake.Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Mock<Response<BlobContentInfo>>(MockBehavior.Strict).Object);
            blobClientFake.Setup(b => b.GetPropertiesAsync(null, It.IsAny<CancellationToken>())).ThrowsAsync(new RequestFailedException(404, "not found"));

            var sut = new DistributedLockManager(blobClientFake.Object, options, loggerMock.Object);

            // Act
            var result = await sut.AcquireAsync(CancellationToken.None);

            // Assert
            result.Should().Be(leaseId);
        }

        [Fact]
        public async Task Given_a_DistributedLockManager_when_AcquireAsync_is_called_for_a_blob_and_container_that_does_not_exist_then_the_lease_is_acquired_correctly()
        {
            // Arrange
            var leaseId = "someId";
            var blobPropertiesFake = BlobsModelFactory.BlobProperties(leaseState: LeaseState.Available);
            var blobLeaseFake = BlobsModelFactory.BlobLease(new ETag(), DateTimeOffset.Now, leaseId: leaseId);
            var blobClientFake = new Mock<BlobClient>(MockBehavior.Strict);
            var blobLeaseClientFake = new Mock<BlobLeaseClient>(MockBehavior.Strict);
            var blobContainerClientFake = new Mock<BlobContainerClient>(MockBehavior.Strict);
            var blobLeaseResponseFake = new Mock<Response<BlobLease>>(MockBehavior.Strict);
            var options = new SingletonHostOptions();
            var loggerMock = new Mock<ILogger>();

            blobLeaseResponseFake.SetupGet(p => p.Value).Returns(blobLeaseFake);
            blobLeaseClientFake.Setup(b => b.AcquireAsync(TimeSpan.FromSeconds(60), null, It.IsAny<CancellationToken>())).ReturnsAsync(blobLeaseResponseFake.Object);

            blobClientFake.Protected().Setup<BlobLeaseClient>("GetBlobLeaseClientCore", ItExpr.IsAny<string>())
                .Returns<string>((leaseId) => blobLeaseClientFake.Object);
            blobClientFake.Setup(b => b.GetPropertiesAsync(null, It.IsAny<CancellationToken>())).ThrowsAsync(new RequestFailedException(404, "blob not found"));
            blobClientFake.SetupSequence(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(404, "container not found"))
                .ReturnsAsync(new Mock<Response<BlobContentInfo>>(MockBehavior.Strict).Object);

            blobContainerClientFake.Setup(c => c.CreateAsync(PublicAccessType.None, null, null, It.IsAny<CancellationToken>())).ReturnsAsync(new Mock<Response<BlobContainerInfo>>().Object);
            blobClientFake.Protected().Setup<BlobContainerClient>("GetParentBlobContainerClientCore")
                .Returns(() => blobContainerClientFake.Object);

            var sut = new DistributedLockManager(blobClientFake.Object, options, loggerMock.Object);

            // Act
            var result = await sut.AcquireAsync(CancellationToken.None);

            // Assert
            result.Should().Be(leaseId);
        }

        [Fact]
        public async Task Given_a_DistributedLockManager_when_AcquireAsync_is_called_and_the_created_container_already_exists_then_the_lease_is_acquired_correctly()
        {
            // Arrange
            var leaseId = "someId";
            var blobPropertiesFake = BlobsModelFactory.BlobProperties(leaseState: LeaseState.Available);
            var blobLeaseFake = BlobsModelFactory.BlobLease(new ETag(), DateTimeOffset.Now, leaseId: leaseId);
            var blobClientFake = new Mock<BlobClient>(MockBehavior.Strict);
            var blobLeaseClientFake = new Mock<BlobLeaseClient>(MockBehavior.Strict);
            var blobContainerClientFake = new Mock<BlobContainerClient>(MockBehavior.Strict);
            var blobLeaseResponseFake = new Mock<Response<BlobLease>>(MockBehavior.Strict);
            var options = new SingletonHostOptions();
            var loggerMock = new Mock<ILogger>();

            blobLeaseResponseFake.SetupGet(p => p.Value).Returns(blobLeaseFake);
            blobLeaseClientFake.Setup(b => b.AcquireAsync(TimeSpan.FromSeconds(60), null, It.IsAny<CancellationToken>())).ReturnsAsync(blobLeaseResponseFake.Object);

            blobClientFake.Protected().Setup<BlobLeaseClient>("GetBlobLeaseClientCore", ItExpr.IsAny<string>())
                .Returns<string>((leaseId) => blobLeaseClientFake.Object);
            blobClientFake.Setup(b => b.GetPropertiesAsync(null, It.IsAny<CancellationToken>())).ThrowsAsync(new RequestFailedException(404, "blob not found"));
            blobClientFake.SetupSequence(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(404, "container not found"))
                .ReturnsAsync(new Mock<Response<BlobContentInfo>>(MockBehavior.Strict).Object);

            blobContainerClientFake.Setup(c => c.CreateAsync(PublicAccessType.None, null, null, It.IsAny<CancellationToken>())).ThrowsAsync(new RequestFailedException(409, "container already exists"));
            blobClientFake.Protected().Setup<BlobContainerClient>("GetParentBlobContainerClientCore")
                .Returns(() => blobContainerClientFake.Object);

            var sut = new DistributedLockManager(blobClientFake.Object, options, loggerMock.Object);

            // Act
            var result = await sut.AcquireAsync(CancellationToken.None);

            // Assert
            result.Should().Be(leaseId);
        }

        [Theory]
        [InlineData(409)]
        [InlineData(412)]
        public async Task Given_a_DistributedLockManager_when_AcquireAsync_is_called_for_a_blob_that_does_not_exist_and_is_concurrently_leased_and_exceptions_occurres_then_it_is_handled_correctly(int statusCode)
        {
            // Arrange
            var leaseId = "someId";
            var blobPropertiesFake = BlobsModelFactory.BlobProperties(leaseState: LeaseState.Available);
            var blobLeaseFake = BlobsModelFactory.BlobLease(new ETag(), DateTimeOffset.Now, leaseId: leaseId);
            var blobClientFake = new Mock<BlobClient>(MockBehavior.Strict);
            var blobLeaseClientFake = new Mock<BlobLeaseClient>(MockBehavior.Strict);
            var blobLeaseResponseFake = new Mock<Response<BlobLease>>(MockBehavior.Strict);
            var options = new SingletonHostOptions();
            var loggerMock = new Mock<ILogger>();

            blobLeaseResponseFake.SetupGet(p => p.Value).Returns(blobLeaseFake);
            blobLeaseClientFake.Setup(b => b.AcquireAsync(TimeSpan.FromSeconds(60), null, It.IsAny<CancellationToken>())).ReturnsAsync(blobLeaseResponseFake.Object);

            blobClientFake.Protected().Setup<BlobLeaseClient>("GetBlobLeaseClientCore", ItExpr.IsAny<string>())
                .Returns<string>((leaseId) => blobLeaseClientFake.Object);
            blobClientFake.Setup(b => b.GetPropertiesAsync(null, It.IsAny<CancellationToken>())).ThrowsAsync(new RequestFailedException(404, "blob not found"));
            blobClientFake.SetupSequence(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(statusCode, "conflict"))
                .ReturnsAsync(new Mock<Response<BlobContentInfo>>(MockBehavior.Strict).Object);

            var sut = new DistributedLockManager(blobClientFake.Object, options, loggerMock.Object);

            // Act
            var result = await sut.AcquireAsync(CancellationToken.None);

            // Assert
            result.Should().Be(leaseId);
        }

        [Fact]
        public async Task Given_a_LockManager_when_RenewAsync_is_called_for_a_blob_that_is_Leased_then_the_lease_is_renewed()
        {
            // Arrange
            var leaseId = "someId";
            var leaseDuration = TimeSpan.FromSeconds(60);
            var blobPropertiesFake = BlobsModelFactory.BlobProperties(leaseState: LeaseState.Leased);
            var blobLeaseFake = BlobsModelFactory.BlobLease(new ETag(), DateTimeOffset.Now, leaseId: leaseId);
            var blobPropertiesResponseFake = new Mock<Response<BlobProperties>>(MockBehavior.Strict);
            var blobClientFake = new Mock<BlobClient>(MockBehavior.Strict);
            var blobLeaseClientFake = new Mock<BlobLeaseClient>(MockBehavior.Strict);
            var blobLeaseResponseFake = new Mock<Response<BlobLease>>(MockBehavior.Strict);
            var options = new SingletonHostOptions();
            var loggerMock = new Mock<ILogger>();

            blobPropertiesResponseFake.SetupGet(p => p.Value).Returns(blobPropertiesFake);
            blobLeaseResponseFake.SetupGet(p => p.Value).Returns(blobLeaseFake);
            blobLeaseClientFake.Setup(b => b.RenewAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(blobLeaseResponseFake.Object);

            blobClientFake.Protected().Setup<BlobLeaseClient>("GetBlobLeaseClientCore", ItExpr.IsAny<string>())
                .Returns<string>((leaseId) => blobLeaseClientFake.Object);
            blobClientFake.Setup(b => b.GetPropertiesAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(blobPropertiesResponseFake.Object);

            var sut = new DistributedLockManager(blobClientFake.Object, options, loggerMock.Object);

            // Act
            var nextTimeout = await sut.RenewAsync(leaseId, CancellationToken.None);

            // Assert
            nextTimeout.Should().BeCloseTo(DateTimeOffset.UtcNow.Add(leaseDuration), TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task Given_a_LockManager_when_RenewAsync_is_called_for_a_blob_that_is_NOT_Leased_then_an_SingletonException_is_thrown()
        {
            // Arrange
            var leaseId = "someId";
            var leaseDuration = TimeSpan.FromSeconds(60);
            var blobPropertiesFake = BlobsModelFactory.BlobProperties(leaseState: LeaseState.Broken);
            var blobLeaseFake = BlobsModelFactory.BlobLease(new ETag(), DateTimeOffset.Now, leaseId: leaseId);
            var blobPropertiesResponseFake = new Mock<Response<BlobProperties>>(MockBehavior.Strict);
            var blobClientFake = new Mock<BlobClient>(MockBehavior.Strict);
            var loggerMock = new Mock<ILogger>();
            var options = new SingletonHostOptions();

            blobClientFake.SetupGet(c => c.Name).Returns("some file name");
            blobPropertiesResponseFake.SetupGet(p => p.Value).Returns(blobPropertiesFake);
            blobClientFake.Setup(b => b.GetPropertiesAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(blobPropertiesResponseFake.Object);

            var sut = new DistributedLockManager(blobClientFake.Object, options, loggerMock.Object);

            // Act
            Func<Task> act = () => sut.RenewAsync(leaseId, CancellationToken.None);

            // Assert
            await act.Should().ThrowExactlyAsync<SingletonException>().WithMessage($"Unable to renew the lock for {blobClientFake.Object.Name} because the lease is not active anymore");
        }

        [Fact]
        public async Task Given_a_LockManager_when_RenewAsync_is_called_and_a_RequestFailedException_is_thrown_then_it_is_logged_and_rethrown_correctly()
        {
            // Arrange
            var leaseId = "someId";
            var leaseDuration = TimeSpan.FromSeconds(60);
            var blobPropertiesFake = BlobsModelFactory.BlobProperties(leaseState: LeaseState.Leased);
            var blobLeaseFake = BlobsModelFactory.BlobLease(new ETag(), DateTimeOffset.Now, leaseId: leaseId);
            var blobPropertiesResponseFake = new Mock<Response<BlobProperties>>(MockBehavior.Strict);
            var blobClientFake = new Mock<BlobClient>(MockBehavior.Strict);
            var blobLeaseClientFake = new Mock<BlobLeaseClient>(MockBehavior.Strict);
            var blobLeaseResponseFake = new Mock<Response<BlobLease>>(MockBehavior.Strict);
            var options = new SingletonHostOptions();
            var loggerMock = new Mock<ILogger>();

            blobClientFake.SetupGet(c => c.Name).Returns("some file name");
            blobPropertiesResponseFake.SetupGet(p => p.Value).Returns(blobPropertiesFake);
            blobLeaseResponseFake.SetupGet(p => p.Value).Returns(blobLeaseFake);
            blobLeaseClientFake.Setup(b => b.RenewAsync(null, It.IsAny<CancellationToken>())).ThrowsAsync(new RequestFailedException(409, "some conflict"));

            blobClientFake.Protected().Setup<BlobLeaseClient>("GetBlobLeaseClientCore", ItExpr.IsAny<string>())
                .Returns<string>((leaseId) => blobLeaseClientFake.Object);
            blobClientFake.Setup(b => b.GetPropertiesAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(blobPropertiesResponseFake.Object);

            loggerMock.Setup(
               x => x.Log(
               It.Is<LogLevel>(l => l == LogLevel.Error),
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"An error occurred while acquiring the lease for blob '{blobClientFake.Object.Name}'")),
               It.IsAny<RequestFailedException>(),
               It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)))
                .Verifiable();

            var sut = new DistributedLockManager(blobClientFake.Object, options, loggerMock.Object);

            // Act
            Func<Task> act = () => sut.RenewAsync(leaseId, CancellationToken.None);

            // Assert
            await act.Should().ThrowExactlyAsync<RequestFailedException>();
            loggerMock.Verify();
        }

        [Fact]
        public async Task Given_a_LockManager_when_ReleaseAsync_is_called_for_a_blob_that_is_Leased_then_the_lease_is_renewed()
        {
            // Arrange
            var leaseId = "someId";
            var leaseDuration = TimeSpan.FromSeconds(60);
            var blobLeaseFake = BlobsModelFactory.BlobLease(new ETag(), DateTimeOffset.Now, leaseId: leaseId);
            var blobClientFake = new Mock<BlobClient>(MockBehavior.Strict);
            var blobLeaseClientFake = new Mock<BlobLeaseClient>(MockBehavior.Strict);
            var blobLeaseResponseFake = new Mock<Response<ReleasedObjectInfo>>(MockBehavior.Strict);
            var options = new SingletonHostOptions();
            var loggerMock = new Mock<ILogger>();

            blobLeaseClientFake.Setup(b => b.ReleaseAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(blobLeaseResponseFake.Object).Verifiable();
            blobClientFake.Protected().Setup<BlobLeaseClient>("GetBlobLeaseClientCore", ItExpr.IsAny<string>())
                .Returns<string>((leaseId) => blobLeaseClientFake.Object);

            var sut = new DistributedLockManager(blobClientFake.Object, options, loggerMock.Object);

            // Act
            await sut.ReleaseAsync(leaseId, CancellationToken.None);

            // Assert
            blobLeaseClientFake.Verify();
        }

        [Theory]
        [InlineData(404)]
        [InlineData(409)]
        public async Task Given_a_LockManager_when_ReleaseAsync_is_called_and_the_blob_does_not_exists_or_is_leased_by_somebody_else_then_the_exception_is_handled(int statusCode)
        {
            // Arrange
            var leaseId = "someId";
            var leaseDuration = TimeSpan.FromSeconds(60);
            var blobLeaseFake = BlobsModelFactory.BlobLease(new ETag(), DateTimeOffset.Now, leaseId: leaseId);
            var blobClientFake = new Mock<BlobClient>(MockBehavior.Strict);
            var blobLeaseClientFake = new Mock<BlobLeaseClient>(MockBehavior.Strict);
            var options = new SingletonHostOptions();
            var loggerMock = new Mock<ILogger>();

            blobLeaseClientFake.Setup(b => b.ReleaseAsync(null, It.IsAny<CancellationToken>())).ThrowsAsync(new RequestFailedException(statusCode, "some message")).Verifiable();
            blobClientFake.Protected().Setup<BlobLeaseClient>("GetBlobLeaseClientCore", ItExpr.IsAny<string>())
                .Returns<string>((leaseId) => blobLeaseClientFake.Object);

            var sut = new DistributedLockManager(blobClientFake.Object, options, loggerMock.Object);

            // Act
            await sut.ReleaseAsync(leaseId, CancellationToken.None);

            // Assert
            blobLeaseClientFake.Verify();
        }

        [Fact]
        public async Task Given_a_LockManager_when_ReleaseAsync_is_called_and_an_server_error_occurrs_then_the_exception_is_thrown()
        {
            // Arrange
            var leaseId = "someId";
            var leaseDuration = TimeSpan.FromSeconds(60);
            var blobLeaseFake = BlobsModelFactory.BlobLease(new ETag(), DateTimeOffset.Now, leaseId: leaseId);
            var blobClientFake = new Mock<BlobClient>(MockBehavior.Strict);
            var blobLeaseClientFake = new Mock<BlobLeaseClient>(MockBehavior.Strict);
            var blobLeaseResponseFake = new Mock<Response<ReleasedObjectInfo>>(MockBehavior.Strict);
            var options = new SingletonHostOptions();
            var loggerMock = new Mock<ILogger>();

            blobLeaseClientFake.Setup(b => b.ReleaseAsync(null, It.IsAny<CancellationToken>())).ThrowsAsync(new RequestFailedException(500, "server error"));
            blobClientFake.Protected().Setup<BlobLeaseClient>("GetBlobLeaseClientCore", ItExpr.IsAny<string>())
                .Returns<string>((leaseId) => blobLeaseClientFake.Object);

            var sut = new DistributedLockManager(blobClientFake.Object, options, loggerMock.Object);

            // Act
            Func<Task> act = () => sut.ReleaseAsync(leaseId, CancellationToken.None);

            // Assert
            await act.Should().ThrowExactlyAsync<RequestFailedException>();
        }
    }
}

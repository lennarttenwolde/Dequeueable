using Dequeueable.Configurations;
using Dequeueable.Services.DistributedLock;
using Dequeueable.Services.Queues;
using Dequeueable.UnitTests.TestDataBuilders;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dequeueable.UnitTests.Services.Singleton
{
    public class DistributedLockQueueMessageExecutorTests
    {
        private readonly string _propertyName = "MyProperty";
        private readonly string _scope = "this is a valid scope";
        private readonly string _leaseId = "someId";

        private (DistributedLockQueueMessageExecutor sut, IDistributedLockManager lockManager, IQueueMessageExecutor executor) CreateSut(DistributedLockOptions? options = null)
        {
            var lockManager = Substitute.For<IDistributedLockManager>();
            var executor = Substitute.For<IQueueMessageExecutor>();
            var distributedLockOptions = options ?? new DistributedLockOptions { Scope = _propertyName };
            var sut = new DistributedLockQueueMessageExecutor(lockManager, executor, TimeProvider.System, Options.Create(distributedLockOptions));
            return (sut, lockManager, executor);
        }

        [Fact]
        public async Task Given_a_DistributedLockQueueMessageExecutor_when_ExecuteAsync_is_called_when_the_singleton_scope_is_null_then_an_InvalidOperationException_is_thrown()
        {
            // Arrange
            var message = new MessageTestDataBuilder().Build();
            var (sut, _, _) = CreateSut(new DistributedLockOptions { Scope = null! });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExecuteAsync(message, CancellationToken.None));
            Assert.Equal("The Singleton Scope cannot be empty when creating a scoped distributed lock", ex.Message);
        }

        [Fact]
        public async Task Given_a_DistributedLockQueueMessageExecutor_when_ExecuteAsync_is_called_with_the_scope_is_does_not_exist_in_the_body_then_a_SingletonException_is_thrown()
        {
            // Arrange
            var message = new MessageTestDataBuilder().WithBody("{\"KeyDoesNotExist\": \"nothing here\"}").Build();
            var (sut, _, _) = CreateSut();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<DistributedLockException>(() => sut.ExecuteAsync(message, CancellationToken.None));
            Assert.Equal($"The provided scope name, '{_propertyName}' , does not exist on the message with id '{message.MessageId}'", ex.Message);
        }

        [Fact]
        public async Task Given_a_DistributedLockQueueMessageExecutor_when_ExecuteAsync_is_called_and_the_body_is_not_valid_JSON_then_a_SingletonException_is_thrown()
        {
            // Arrange
            var message = new MessageTestDataBuilder().WithBody("this is no jason!").Build();
            var (sut, _, _) = CreateSut();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<DistributedLockException>(() => sut.ExecuteAsync(message, CancellationToken.None));
            Assert.Equal($"Unable to parse the body for the message with id '{message.MessageId}'", ex.Message);
        }

        [Fact]
        public async Task Given_a_DistributedLockQueueMessageExecutor_when_ExecuteAsync_is_called_and_the_parsed_property_is_empty_then_a_SingletonException_is_thrown()
        {
            // Arrange
            var message = new MessageTestDataBuilder().WithBody($"{{\"{_propertyName}\": \"\"}}").Build();
            var (sut, _, _) = CreateSut();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<DistributedLockException>(() => sut.ExecuteAsync(message, CancellationToken.None));
            Assert.Equal($"The provided scope name, '{_propertyName}' , does not exist on the message with id '{message.MessageId}'", ex.Message);
        }

        [Fact]
        public async Task Given_a_DistributedLockQueueMessageExecutor_when_ExecuteAsync_is_called_and_the_lease_cannot_be_renewed_then_a_SingletonException_is_thrown()
        {
            // Arrange
            var message = new MessageTestDataBuilder().WithBody($"{{\"{_propertyName}\": \"{_scope}\"}}").Build();
            var (sut, lockManager, executor) = CreateSut();

            lockManager.AquireLockAsync(_scope, Arg.Any<CancellationToken>()).Returns(_leaseId);
            lockManager.ReleaseLockAsync(_leaseId, _scope, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            lockManager.RenewLockAsync(_leaseId, _scope, Arg.Any<CancellationToken>()).ThrowsAsync(new Exception("Renew fails!"));
            executor.ExecuteAsync(message, Arg.Any<CancellationToken>()).Returns(Task.Delay(TimeSpan.FromSeconds(10)));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<DistributedLockException>(() => sut.ExecuteAsync(message, CancellationToken.None));
            Assert.Equal($"Unable to renew the lease with id '{_leaseId}'. Distributed lock cannot be guaranteed.", ex.Message);
        }

        [Fact]
        public async Task Given_a_DistributedLockQueueMessageExecutor_when_ExecuteAsync_is_called_and_an_exception_occurs_when_executing_the_message_then_it_is_rethrown()
        {
            // Arrange
            var message = new MessageTestDataBuilder().WithBody($"{{\"{_propertyName}\": \"{_scope}\"}}").Build();
            var (sut, lockManager, executor) = CreateSut();

            lockManager.AquireLockAsync(_scope, Arg.Any<CancellationToken>()).Returns(_leaseId);
            lockManager.ReleaseLockAsync(_leaseId, _scope, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            lockManager.RenewLockAsync(_leaseId, _scope, Arg.Any<CancellationToken>()).Returns(DateTimeOffset.UtcNow.AddSeconds(60));
            executor.ExecuteAsync(message, Arg.Any<CancellationToken>()).ThrowsAsync(new Exception());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => sut.ExecuteAsync(message, CancellationToken.None));
        }

        [Fact]
        public async Task Given_a_DistributedLockQueueMessageExecutor_when_ExecuteAsync_is_called_then_the_message_is_executed_correctly()
        {
            // Arrange
            var message = new MessageTestDataBuilder().WithBody($"{{\"{_propertyName}\": \"{_scope}\"}}").Build();
            var (sut, lockManager, executor) = CreateSut();

            lockManager.AquireLockAsync(_scope, Arg.Any<CancellationToken>()).Returns(_leaseId);
            lockManager.ReleaseLockAsync(_leaseId, _scope, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            lockManager.RenewLockAsync(_leaseId, _scope, Arg.Any<CancellationToken>()).Returns(DateTimeOffset.UtcNow.AddSeconds(60));
            executor.ExecuteAsync(message, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

            // Act
            await sut.ExecuteAsync(message, CancellationToken.None);

            // Assert
            await executor.Received(1).ExecuteAsync(message, Arg.Any<CancellationToken>());
        }
    }
}
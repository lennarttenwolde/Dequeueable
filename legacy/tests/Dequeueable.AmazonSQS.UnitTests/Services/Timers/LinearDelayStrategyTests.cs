using Dequeueable.AzureQueueStorage.Services.Timers;
using FluentAssertions;

namespace Dequeueable.AmazonSQS.UnitTests.Services.Timers
{
    public class LinearDelayStrategyTests
    {
        [Fact]
        public void Given_a_LinearDelayStrategy_when_GetNextDelay_is_called_with_executionSucceeded_false_then_the_MinimalRenewalDelay_is_returned()
        {
            // Arrange
            var executionSucceeded = false;
            var minimalRenewalDelay = TimeSpan.FromSeconds(1);

            var sut = new LinearDelayStrategy(minimalRenewalDelay);

            // Act
            var delay = sut.GetNextDelay(executionSucceeded: executionSucceeded);

            // Assert
            delay.Should().Be(minimalRenewalDelay);
        }

        [Fact]
        public void Given_a_LinearDelayStrategy_when_GetNextDelay_is_called_with_nextVisibleOn_null_then_the_MinimalRenewalDelay_is_returned()
        {
            // Arrange
            var minimalRenewalDelay = TimeSpan.FromSeconds(1);

            var sut = new LinearDelayStrategy(minimalRenewalDelay);

            // Act
            var delay = sut.GetNextDelay();

            // Assert
            delay.Should().Be(minimalRenewalDelay);
        }

        [Fact]
        public void Given_a_LinearDelayStrategy_when_GetNextDelay_is_called_with_a_positive_nextVisibleOn_then_the_MinimalRenewalDelay_is_returned()
        {
            // Arrange
            var minimalRenewalDelay = TimeSpan.FromSeconds(1);

            var sut = new LinearDelayStrategy(minimalRenewalDelay)
            {
                Divisor = 2
            };

            // Act
            var delay = sut.GetNextDelay(nextVisibleOn: DateTimeOffset.UtcNow.Add(TimeSpan.FromSeconds(60)));

            // Assert
            delay.Should().BeCloseTo(TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(6));
        }
    }
}

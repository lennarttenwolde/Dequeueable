using Dequeueable.AmazonSQS.Services.Timers;
using FluentAssertions;

namespace Dequeueable.AmazonSQS.UnitTests.Services.Timers
{
    public class RandomizedExponentialDelayStrategyTests
    {
        [Fact]
        public void Given_a_RandomizedExponentialDelayStrategy_when_constructed_with_a_minimumInterval_lower_than_zero_then_an_ArgumentOutOfRangeException_is_thrown()
        {
            // Arrange
            var minimumPollingInterval = TimeSpan.FromMilliseconds(-1);
            var maximumPollingInterval = TimeSpan.FromMilliseconds(2);

            // Act
            Action act = () => { var _ = new RandomizedExponentialDelayStrategy(minimumPollingInterval, maximumPollingInterval); };

            // Assert
            act.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Given_a_RandomizedExponentialDelayStrategy_when_constructed_with_a_minimumInterval_higer_than_the_maximumInterval_then_an_ArgumentException_is_thrown()
        {
            // Arrange
            var minimumPollingInterval = TimeSpan.FromMilliseconds(2);
            var maximumPollingInterval = TimeSpan.FromMilliseconds(1);

            // Act
            Action act = () => { var _ = new RandomizedExponentialDelayStrategy(minimumPollingInterval, maximumPollingInterval); };

            // Assert
            act.Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void Given_a_RandomizedExponentialDelayStrategy_when_constructed_with_a_maximumInterval_lower_than_zero_then_an_ArgumentOutOfRangeException_is_thrown()
        {
            // Arrange
            var minimumPollingInterval = TimeSpan.FromMilliseconds(1);
            var maximumPollingInterval = TimeSpan.FromMilliseconds(-2);

            // Act
            Action act = () => { var _ = new RandomizedExponentialDelayStrategy(minimumPollingInterval, maximumPollingInterval); };

            // Assert
            act.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Given_a_RandomizedExponentialDelayStrategy_when_constructed_with_a_maximumInterval_lower_than_the_minimumInterval_then_an_ArgumentException_is_thrown()
        {
            // Arrange
            var minimumPollingInterval = TimeSpan.FromMilliseconds(2);
            var maximumPollingInterval = TimeSpan.FromMilliseconds(1);

            // Act
            Action act = () => { var _ = new RandomizedExponentialDelayStrategy(minimumPollingInterval, maximumPollingInterval); };

            // Assert
            act.Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void Given_a_RandomizedExponentialDelayStrategy_when_GetNextDelay_is_called_with_executionSucceeded_true_then_the_correct_result_TimeSpan_is_returned()
        {
            // Arrange
            var executionSucceeded = true;
            var minimumPollingInterval = TimeSpan.FromMilliseconds(1);
            var maximumPollingInterval = TimeSpan.FromMilliseconds(2);

            var sut = new RandomizedExponentialDelayStrategy(minimumPollingInterval, maximumPollingInterval);

            // Act
            var actual = sut.GetNextDelay(executionSucceeded: executionSucceeded);

            // Assert
            actual.Should().Be(minimumPollingInterval);
        }

        [Fact]
        public void Given_a_RandomizedExponentialBackoffStrategy_when_NextDelay_is_called_multiple_times_with_executionSucceeded_false_then_the_TimeSpan_increment_correctly()
        {
            // Arrange
            var executionSucceeded = false;
            var minimumPollingInterval = TimeSpan.FromMilliseconds(1);
            var maximumPollingInterval = TimeSpan.FromMilliseconds(500);

            var sut = new RandomizedExponentialDelayStrategy(minimumPollingInterval, maximumPollingInterval);

            // Act & Assert
            var currentInterval = TimeSpan.Zero;
            while (currentInterval != maximumPollingInterval)
            {
                var actual = sut.GetNextDelay(executionSucceeded: executionSucceeded);
                actual.Should().BeGreaterThan(currentInterval);

                currentInterval = actual;
            }

            currentInterval.Should().Be(maximumPollingInterval);
        }
    }
}

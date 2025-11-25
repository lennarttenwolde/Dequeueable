using Dequeueable.AmazonSQS.Configurations;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace Dequeueable.AmazonSQS.UnitTests.Configurations
{
    public class ListenerHostOptionsTests
    {
        [Fact]
        public void Given_a_ListenerHostOptions_when_MinimumPollingIntervalInMilliseconds_is_zero_then_the_validation_result_contains_the_correct_error_message()
        {
            // Arrange
            var sut = new ListenerHostOptions
            {
                MinimumPollingIntervalInMilliseconds = 0
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().Contain(e => e.ErrorMessage!.Contains("Value for MinimumPollingIntervalInMilliseconds must be lower than 1."));
        }

        [Fact]
        public void Given_a_ListenerHostOptions_when_MinimumPollingIntervalInMilliseconds_is_within_range_then_the_validation_result_are_empty()
        {
            // Arrange
            var sut = new ListenerHostOptions
            {
                MinimumPollingIntervalInMilliseconds = 5
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().NotContain(e => e.MemberNames!.Contains("MinimumPollingIntervalInMilliseconds"));
        }

        [Fact]
        public void Given_a_ListenerHostOptions_when_MaximumPollingIntervalInMilliseconds_is_zero_then_the_validation_result_contains_the_correct_error_message()
        {
            // Arrange
            var sut = new ListenerHostOptions
            {
                MaximumPollingIntervalInMilliseconds = 0
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().Contain(e => e.ErrorMessage!.Contains("Value for MaximumPollingIntervalInMilliseconds must be lower than 1."));
        }

        [Fact]
        public void Given_a_ListenerHostOptions_when_MaximumPollingIntervalInMilliseconds_is_within_range_then_the_validation_result_are_empty()
        {
            // Arrange
            var sut = new ListenerHostOptions
            {
                MaximumPollingIntervalInMilliseconds = 5
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().NotContain(e => e.MemberNames!.Contains("MaximumPollingIntervalInMilliseconds"));
        }

        [Fact]
        public void Given_a_ListenerHostOptions_when_MinimumPollingIntervalInMilliseconds_is_higher_than_MaximumPollingIntervalInMilliseconds_then_ValidatePollingInterval_returns_false()
        {
            // Arrange
            var sut = new ListenerHostOptions
            {
                MaximumPollingIntervalInMilliseconds = 5,
                MinimumPollingIntervalInMilliseconds = 6
            };

            // Act
            var result = ListenerHostOptions.ValidatePollingInterval(sut);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Given_a_ListenerHostOptions_when_MinimumPollingIntervalInMilliseconds_is_lower_than_MaximumPollingIntervalInMilliseconds_then_ValidatePollingInterval_returns_true()
        {
            // Arrange
            var sut = new ListenerHostOptions
            {
                MaximumPollingIntervalInMilliseconds = 6,
                MinimumPollingIntervalInMilliseconds = 5
            };

            // Act
            var result = ListenerHostOptions.ValidatePollingInterval(sut);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Given_a_ListenerHostOptions_when_NewBatchThreshold_is_higher_than_BatchSize_then_ValidateNewBatchThreshold_returns_false()
        {
            // Arrange
            var sut = new ListenerHostOptions
            {
                NewBatchThreshold = 6,
                BatchSize = 4
            };

            // Act
            var result = ListenerHostOptions.ValidateNewBatchThreshold(sut);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Given_a_ListenerHostOptions_when_NewBatchThreshold_is_lower_than_BatchSize_then_ValidateNewBatchThreshold_returns_true()
        {
            // Arrange
            var sut = new ListenerHostOptions
            {
                NewBatchThreshold = 5,
                BatchSize = 5
            };

            // Act
            var result = ListenerHostOptions.ValidateNewBatchThreshold(sut);

            // Assert
            result.Should().BeTrue();
        }

        private static List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }
    }
}

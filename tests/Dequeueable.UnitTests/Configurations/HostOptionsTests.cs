using Dequeueable.AzureQueueStorage.Configurations;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace Dequeueable.AzureQueueStorage.UnitTests.Configurations
{
    public class HostOptionsTests
    {
        [Fact]
        public void Given_a_HostOptions_when_QueueName_is_null_then_the_validation_result_contains_the_correct_error_message()
        {
            // Arrange
            var sut = new HostOptions
            {
                QueueName = null!
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().Contain(e => e.ErrorMessage!.Contains("Value for QueueName cannot be null."));
        }

        [Fact]
        public void Given_a_HostOptions_when_QueueName_is_empty_then_the_validation_result_contains_the_correct_error_message()
        {
            // Arrange
            var sut = new HostOptions
            {
                QueueName = string.Empty
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().NotContain(e => e.MemberNames!.Contains("QueueName"));
        }

        [Fact]
        public void Given_a_HostOptions_when_BatchSize_is_within_range_then_the_validation_result_are_empty()
        {
            // Arrange
            var sut = new HostOptions
            {
                BatchSize = 5
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().NotContain(e => e.MemberNames!.Contains("BatchSize"));
        }

        [Fact]
        public void Given_a_HostOptions_when_BatchSize_is_zero_then_the_validation_result_contains_the_correct_error_message()
        {
            // Arrange
            var sut = new HostOptions
            {
                BatchSize = 0
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().Contain(e => e.ErrorMessage!.Contains("Value for BatchSize must be between 1 and 100."));
        }

        [Fact]
        public void Given_a_HostOptions_when_BatchSize_is_101_then_the_validation_result_contains_the_correct_error_message()
        {
            // Arrange
            var sut = new HostOptions
            {
                BatchSize = 101
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().Contain(e => e.ErrorMessage!.Contains("Value for BatchSize must be between 1 and 100."));
        }

        [Fact]
        public void Given_a_HostOptions_when_MaxDequeueCount_is_within_range_then_the_validation_result_are_empty()
        {
            // Arrange
            var sut = new HostOptions
            {
                MaxDequeueCount = 5
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().NotContain(e => e.MemberNames!.Contains("MaxDequeueCount"));
        }

        [Fact]
        public void Given_a_HostOptions_when_MaxDequeueCount_is_negative_then_the_validation_result_contains_the_correct_error_message()
        {
            // Arrange
            var sut = new HostOptions
            {
                MaxDequeueCount = -1
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().Contain(e => e.ErrorMessage!.Contains("Value for MaxDequeueCount must be between 0 and 20."));
        }

        [Fact]
        public void Given_a_HostOptions_when_MaxDequeueCount_is_21_then_the_validation_result_contains_the_correct_error_message()
        {
            // Arrange
            var sut = new HostOptions
            {
                MaxDequeueCount = 21
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().Contain(e => e.ErrorMessage!.Contains("Value for MaxDequeueCount must be between 0 and 20."));
        }

        [Fact]
        public void Given_a_HostOptions_when_VisibilityTimeoutInSeconds_is_within_range_then_the_validation_result_are_empty()
        {
            // Arrange
            var sut = new HostOptions
            {
                VisibilityTimeoutInSeconds = 5
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().NotContain(e => e.MemberNames!.Contains("VisibilityTimeoutInSeconds"));
        }

        [Fact]
        public void Given_a_HostOptions_when_VisibilityTimeoutInSeconds_is_zero_then_the_validation_result_contains_the_correct_error_message()
        {
            // Arrange
            var sut = new HostOptions
            {
                VisibilityTimeoutInSeconds = 0
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().Contain(e => e.ErrorMessage!.Contains("Value for VisibilityTimeoutInSeconds must not be negative or zero"));
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

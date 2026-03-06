using Dequeueable.AzureQueueStorage.Configurations;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace Dequeueable.AzureQueueStorage.UnitTests.Configurations
{
    public class SingletonHostOptionsTests
    {
        [Fact]
        public void Given_a_SingletonHostOptions_when_the_Scope_is_set_to_empty_then_the_validation_result_contains_the_correct_error_message()
        {
            // Arrange
            var sut = new SingletonHostOptions
            {
                Scope = string.Empty
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().Contain(e => e.ErrorMessage!.Contains("Scope cannot be empty."));
        }

        [Fact]
        public void Given_a_SingletonHostOptions_when_the_MinimumPollingIntervalInSeconds_is_set_to_negative_then_the_validation_result_contains_the_correct_error_message()
        {
            // Arrange
            var sut = new SingletonHostOptions
            {
                MinimumPollingIntervalInSeconds = -1
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().Contain(e => e.ErrorMessage!.Contains("Value for MinimumPollingIntervalInSeconds must not be negative or zero."));
        }

        [Fact]
        public void Given_a_SingletonHostOptions_when_the_MinimumPollingIntervalInSeconds_is_set_within_range_then_the_validation_result_not_contains_the_error_message()
        {
            // Arrange
            var sut = new SingletonHostOptions
            {
                MinimumPollingIntervalInSeconds = 5
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().NotContain(e => e.MemberNames!.Contains("MinimumPollingIntervalInSeconds"));
        }

        [Fact]
        public void Given_a_SingletonHostOptions_when_the_MaximumPollingIntervalInSeconds_is_set_to_zero_then_the_validation_result_contains_the_correct_error_message()
        {
            // Arrange
            var sut = new SingletonHostOptions
            {
                MaximumPollingIntervalInSeconds = 0
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().Contain(e => e.ErrorMessage!.Contains("Value for MaximumPollingIntervalInSeconds must not be negative or zero."));
        }

        [Fact]
        public void Given_a_SingletonHostOptions_when_the_MaximumPollingIntervalInSeconds_is_set_within_range_then_the_validation_result_not_contains_the_error_message()
        {
            // Arrange
            var sut = new SingletonHostOptions
            {
                MaximumPollingIntervalInSeconds = 200
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().NotContain(e => e.MemberNames!.Contains("MaximumPollingIntervalInSeconds"));
        }

        [Fact]
        public void Given_a_SingletonHostOptions_when_the_MaxRetries_is_set_to_negative_then_the_validation_result_contains_the_correct_error_message()
        {
            // Arrange
            var sut = new SingletonHostOptions
            {
                MaxRetries = -1
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().Contain(e => e.ErrorMessage!.Contains("Value for MaxRetries must be between 0 and 10."));
        }

        [Fact]
        public void Given_a_SingletonHostOptions_when_the_MaxRetries_is_set_set_to_11_then_the_validation_result_contains_the_correct_error_message()
        {
            // Arrange
            var sut = new SingletonHostOptions
            {
                MaxRetries = 11
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().Contain(e => e.ErrorMessage!.Contains("Value for MaxRetries must be between 0 and 10."));
        }

        [Fact]
        public void Given_a_SingletonHostOptions_when_the_MaxRetries_is_set_within_range_then_the_validation_result_not_contains_the_error_message()
        {
            // Arrange
            var sut = new SingletonHostOptions
            {
                MaxRetries = 5
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().NotContain(e => e.MemberNames!.Contains("MaxRetries"));
        }

        [Fact]
        public void Given_a_SingletonHostOptions_when_the_ContainerName_is_set_to_an_empty_string_then_the_validation_result_contains_the_correct_error_message()
        {
            // Arrange
            var sut = new SingletonHostOptions
            {
                ContainerName = string.Empty
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().Contain(e => e.ErrorMessage!.Contains("ContainerName cannot be empty."));
        }

        [Fact]
        public void Given_a_SingletonHostOptions_when_the_BlobUriFormat_is_set_to_an_empty_string_then_the_validation_result_contains_the_correct_error_message()
        {
            // Arrange
            var sut = new SingletonHostOptions
            {
                BlobUriFormat = string.Empty
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().Contain(e => e.ErrorMessage!.Contains("BlobUriFormat cannot be empty."));
        }

        [Fact]
        public void Given_a_SingletonHostOptions_when_MinimumPollingIntervalInSeconds_is_lower_than_MaximumPollingIntervalInSeconds_then_ValidateNewBatchThreshold_returns_true()
        {
            // Arrange
            var sut = new SingletonHostOptions
            {
                MinimumPollingIntervalInSeconds = 5,
                MaximumPollingIntervalInSeconds = 6
            };

            // Act
            var result = SingletonHostOptions.ValidatePollingInterval(sut);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Given_a_SingletonHostOptions_when_MinimumPollingIntervalInSeconds_is_higher_than_MaximumPollingIntervalInSeconds_then_ValidateNewBatchThreshold_returns_false()
        {
            // Arrange
            var sut = new SingletonHostOptions
            {
                MinimumPollingIntervalInSeconds = 7,
                MaximumPollingIntervalInSeconds = 6
            };

            // Act
            var result = SingletonHostOptions.ValidatePollingInterval(sut);

            // Assert
            result.Should().BeFalse();
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

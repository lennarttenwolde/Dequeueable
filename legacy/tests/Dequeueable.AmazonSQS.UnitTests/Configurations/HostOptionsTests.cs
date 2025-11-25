using Dequeueable.AmazonSQS.Configurations;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace Dequeueable.AmazonSQS.UnitTests.Configurations
{
    public class HostOptionsTests
    {
        [Fact]
        public void Given_a_HostOptions_when_QueueUrl_is_empty_then_the_validation_result_contains_the_correct_error_message()
        {
            // Arrange
            var sut = new HostOptions
            {
                QueueUrl = string.Empty
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().Contain(e => e.ErrorMessage!.Contains("QueueUrl cannot be empty."));
        }

        [Fact]
        public void Given_a_HostOptions_when_QueueUrl_is_not_empty_then_the_validation_result_is_empty()
        {
            // Arrange
            var sut = new HostOptions
            {
                QueueUrl = "my url"
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().NotContain(e => e.MemberNames!.Contains("QueueUrl"));
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
            result.Should().Contain(e => e.ErrorMessage!.Contains("Value for BatchSize must be between 1 and 10."));
        }

        [Fact]
        public void Given_a_HostOptions_when_BatchSize_is_eleven_then_the_validation_result_contains_the_correct_error_message()
        {
            // Arrange
            var sut = new HostOptions
            {
                BatchSize = 11
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().Contain(e => e.ErrorMessage!.Contains("Value for BatchSize must be between 1 and 10."));
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
        public void Given_a_HostOptions_when_VisibilityTimeoutInSeconds_is_zero_then_the_validation_result_contains_the_correct_error_message()
        {
            // Arrange
            var sut = new HostOptions
            {
                VisibilityTimeoutInSeconds = 29
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().Contain(e => e.ErrorMessage!.Contains("Value for VisibilityTimeoutInSeconds must be between 30 and 43200."));
        }

        [Fact]
        public void Given_a_HostOptions_when_VisibilityTimeoutInSeconds_is_43201_then_the_validation_result_contains_the_correct_error_message()
        {
            // Arrange
            var sut = new HostOptions
            {
                VisibilityTimeoutInSeconds = 43201
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().Contain(e => e.ErrorMessage!.Contains("Value for VisibilityTimeoutInSeconds must be between 30 and 43200."));
        }

        [Fact]
        public void Given_a_HostOptions_when_VisibilityTimeoutInSeconds_is_within_range_then_the_validation_result_are_empty()
        {
            // Arrange
            var sut = new HostOptions
            {
                VisibilityTimeoutInSeconds = 30
            };

            // Act
            var result = ValidateModel(sut);

            // Assert
            result.Should().NotContain(e => e.MemberNames!.Contains("VisibilityTimeoutInSeconds"));
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

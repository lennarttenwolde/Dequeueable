using Dequeueable.AzureQueueStorage.Extentions;
using Dequeueable.AzureQueueStorage.UnitTests.TestDataBuilders;
using FluentAssertions;
using System.Text.Json;

namespace Dequeueable.AzureQueueStorage.UnitTests.Extentions
{
    public class QueueMessageExtentionsTests
    {
        [Theory]
        [InlineData("some value")]
        [InlineData(419)]
        [InlineData("f16aa521-989d-481c-a982-cfbbeece1fa8")]
        [InlineData('3')]
        [InlineData(true)]
        [InlineData(null)]
        public void Given_a_Message_when_GetValueByPropertyName_is_called_with_different_values_then_the_value_is_returned_from_the_parsed_body(object? propertyValue)
        {
            // Arrange
            var propertyName = "MyProperty";
            var body = BinaryData.FromObjectAsJson(new { MyProperty = propertyValue });

            var message = new MessageTestDataBuilder().WithBody(body).Build();

            // Act
            var result = message.GetValueByPropertyName(propertyName);

            // Assert
            result.Should().Be(propertyValue?.ToString() ?? string.Empty);
        }

        [Fact]
        public void Given_a_Message_when_GetValueByPropertyName_is_called_nested_property_then_the_value_is_returned_from_the_parsed_body()
        {
            // Arrange
            var propertyName = "Parent:Nested:Property";
            var propertyValue = "my value";
            var body = BinaryData.FromObjectAsJson(new { Parent = new { Nested = new { Property = propertyValue } } });

            var message = new MessageTestDataBuilder().WithBody(body).Build();

            // Act
            var result = message.GetValueByPropertyName(propertyName);

            // Assert
            result.Should().Be(propertyValue);
        }

        [Fact]
        public void Given_a_Message_when_GetValueByPropertyName_is_called_and_the_value_is_of_type_Object_then_an_InvalidOperationException_is_thrown()
        {
            // Arrange
            var propertyName = "InvalidProperty";
            var body = BinaryData.FromObjectAsJson(new { InvalidProperty = new { ThisIsNotValid = "boom" } });

            var message = new MessageTestDataBuilder().WithBody(body).Build();

            // Act
            Action act = () => message.GetValueByPropertyName(propertyName);

            // Assert
            act.Should().ThrowExactly<InvalidOperationException>().WithMessage($"The value of type {JsonValueKind.Object} cannot be parsed to a string");
        }

        [Fact]
        public void Given_a_Message_when_GetValueByPropertyName_is_called_and_the_value_is_of_type_Array_then_an_InvalidOperationException_is_thrown()
        {
            // Arrange
            var propertyName = "SomeList";
            var body = BinaryData.FromObjectAsJson(new { SomeList = new List<string> { "hey" } });

            var message = new MessageTestDataBuilder().WithBody(body).Build();

            // Act
            Action act = () => message.GetValueByPropertyName(propertyName);

            // Assert
            act.Should().ThrowExactly<InvalidOperationException>().WithMessage($"The value of type {JsonValueKind.Array} cannot be parsed to a string");
        }
    }
}

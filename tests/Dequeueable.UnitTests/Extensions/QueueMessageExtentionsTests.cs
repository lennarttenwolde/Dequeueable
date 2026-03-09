using Dequeueable.Extensions;
using Dequeueable.UnitTests.TestDataBuilders;
using System.Text.Json;

namespace Dequeueable.UnitTests.Extensions
{
    public class QueueMessageExtensionsTests
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
            Assert.Equal(propertyValue?.ToString() ?? string.Empty, result);
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
            Assert.Equal(propertyValue, result);
        }

        [Fact]
        public void Given_a_Message_when_GetValueByPropertyName_is_called_and_the_value_is_of_type_Object_then_an_InvalidOperationException_is_thrown()
        {
            // Arrange
            var propertyName = "InvalidProperty";
            var body = BinaryData.FromObjectAsJson(new { InvalidProperty = new { ThisIsNotValid = "boom" } });
            var message = new MessageTestDataBuilder().WithBody(body).Build();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => message.GetValueByPropertyName(propertyName));
            Assert.Equal($"The value of type {JsonValueKind.Object} cannot be parsed to a string", ex.Message);
        }

        [Fact]
        public void Given_a_Message_when_GetValueByPropertyName_is_called_and_the_value_is_of_type_Array_then_an_InvalidOperationException_is_thrown()
        {
            // Arrange
            var propertyName = "SomeList";
            var body = BinaryData.FromObjectAsJson(new { SomeList = new List<string> { "hey" } });
            var message = new MessageTestDataBuilder().WithBody(body).Build();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => message.GetValueByPropertyName(propertyName));
            Assert.Equal($"The value of type {JsonValueKind.Array} cannot be parsed to a string", ex.Message);
        }
    }
}
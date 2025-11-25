using Dequeueable.AmazonSQS.UnitTests.TestDataBuilders;
using FluentAssertions;

namespace Dequeueable.AmazonSQS.UnitTests.Models
{
    public class MessageTests
    {
        [Fact]
        public void Given_a_Message_when_Attibues_has_a_MessageGroupId_then_MessageGroupId_contains_the_correct_value()
        {
            // Arrange
            var attributes = new Dictionary<string, string> {
                {"MessageGroupId", "value" }
            };
            var sut = new MessageTestDataBuilder().WithAttributes(attributes).Build();

            // Act
            var result = sut.MessageGroupId;

            // Assert
            result.Should().Be(attributes["MessageGroupId"]);
        }

        [Fact]
        public void Given_a_Message_when_Attibues_has_a_no_MessageGroupId_then_MessageGroupId_is_null()
        {
            // Arrange
            var attributes = new Dictionary<string, string> { };
            var sut = new MessageTestDataBuilder().WithAttributes(attributes).Build();

            // Act
            var result = sut.MessageGroupId;

            // Assert
            result.Should().BeNull();
        }
    }
}

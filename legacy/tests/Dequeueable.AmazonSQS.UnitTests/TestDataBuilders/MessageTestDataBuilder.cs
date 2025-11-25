using Dequeueable.AmazonSQS.Models;

namespace Dequeueable.AmazonSQS.UnitTests.TestDataBuilders
{
    public class MessageTestDataBuilder
    {
        private string _messageId = "some id";
        private readonly string _receiptHandle = "some pop";
        private DateTimeOffset _nextVisibileOn = DateTimeOffset.UtcNow.AddMinutes(1);
        private BinaryData _body = BinaryData.FromString("test body");
        private Dictionary<string, string> _attributes = new();

        public Message Build()
        {
            return new Message(_messageId, _receiptHandle, _nextVisibileOn, _body, _attributes);
        }

        public MessageTestDataBuilder WithmessageId(string messageId)
        {
            _messageId = messageId;
            return this;
        }

        public MessageTestDataBuilder WithNextVisibileOn(DateTimeOffset nextVisibileOn)
        {
            _nextVisibileOn = nextVisibileOn;
            return this;
        }

        public MessageTestDataBuilder WithBody(string body)
        {
            _body = BinaryData.FromString(body);
            return this;
        }

        public MessageTestDataBuilder WithBody(BinaryData body)
        {
            _body = body;
            return this;
        }

        public MessageTestDataBuilder WithAttributes(Dictionary<string, string> attributes)
        {
            _attributes = attributes;
            return this;
        }
    }
}

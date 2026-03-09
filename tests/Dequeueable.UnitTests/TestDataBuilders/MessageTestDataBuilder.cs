using Dequeueable.Models;

namespace Dequeueable.UnitTests.TestDataBuilders
{
#pragma warning disable CA1515 // Consider making public types internal
    public class MessageTestDataBuilder
#pragma warning restore CA1515 // Consider making public types internal
    {
        private string _messageId = "some id";
        private readonly string _popReceipt = "some pop";
        private long _dequeueCount = 5;
        private DateTimeOffset? _nextVisibileOn = DateTimeOffset.UtcNow.AddMinutes(1);
        private BinaryData _body = BinaryData.FromString("test body");

        public Message Build()
        {
            return new Message(_messageId, _popReceipt, _dequeueCount, _nextVisibileOn, _body);
        }

        public MessageTestDataBuilder WithmessageId(string messageId)
        {
            _messageId = messageId;
            return this;
        }

        public MessageTestDataBuilder WithDequeueCount(int dequeueCount)
        {
            _dequeueCount = dequeueCount;
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
    }
}

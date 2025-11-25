namespace Dequeueable.AmazonSQS.Models
{
    /// <summary>
    /// Queue message retrieved from the qeueue.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// The unqiue id of the message
        /// </summary>
        public string MessageId { get; }

        /// <summary>
        /// MessageGroupId is the tag that specifies that a message belongs to a specific message group. This is used as scope for singletons. 
        /// </summary>
        public string? MessageGroupId => Attributes.GetValueOrDefault("MessageGroupId");

        /// <summary>
        /// Unique receipt of the Queue Message. 
        /// </summary>
        public string ReceiptHandle { get; internal set; }

        /// <summary>
        /// <see cref="DateTimeOffset"/> of the queue message when it is visibile again for other clients.
        /// </summary>
        public DateTimeOffset NextVisibleOn { get; }

        /// <summary>
        /// <see cref="BinaryData"/> of the body.
        /// </summary>
        public BinaryData Body { get; }

        /// <summary>
        /// A list of attributes that need to be returned along with each message <see cref="Amazon.SQS.Model.Message.Attributes"/>.
        /// </summary>
        public Dictionary<string, string> Attributes { get; } = [];

        /// <summary>
        /// Creates an instance of the queue message.
        /// </summary>
        /// <param name="messageId">Id of the Queue Message. </param>
        /// <param name="receiptHandle">Unique receipt of the Queue Message. </param>
        /// <param name="nextVisibleOn"> <see cref="DateTimeOffset"/> of the queue message when it is visibile again for other clients.</param>
        /// <param name="body"><see cref="BinaryData"/> of the body.</param>
        public Message(string messageId, string receiptHandle, DateTimeOffset nextVisibleOn, BinaryData body)
        {
            MessageId = messageId;
            ReceiptHandle = receiptHandle;
            NextVisibleOn = nextVisibleOn;
            Body = body;
        }

        /// <summary>
        /// Creates an instance of the queue message.
        /// </summary>
        /// <param name="messageId">Id the Queue Message. </param>
        /// <param name="receiptHandle">Unique receipt of the Queue Message. </param>
        /// <param name="nextVisibleOn"> <see cref="DateTimeOffset"/> of the queue message when it is visibile again for other clients.</param>
        /// <param name="body"><see cref="BinaryData"/> of the body.</param>
        /// <param name="attributes">A list of attributes that need to be returned along with each message <see cref="Amazon.SQS.Model.Message.Attributes"/>.</param>
        public Message(string messageId, string receiptHandle, DateTimeOffset nextVisibleOn, BinaryData body, Dictionary<string, string> attributes)
        {
            MessageId = messageId;
            ReceiptHandle = receiptHandle;
            NextVisibleOn = nextVisibleOn;
            Body = body;
            Attributes = attributes;
        }
    }
}

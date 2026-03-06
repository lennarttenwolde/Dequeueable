namespace Dequeueable.AzureQueueStorage.Services.Queues
{
    /// <summary>
    /// Represents an exception that occurs when there is an issue with setting the visibility timeout for a queue message.
    /// </summary>
    public class VisibilityTimeoutException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VisibilityTimeoutException"/> class.
        /// </summary>
        public VisibilityTimeoutException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisibilityTimeoutException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public VisibilityTimeoutException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisibilityTimeoutException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public VisibilityTimeoutException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}

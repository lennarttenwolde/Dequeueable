namespace Dequeueable.AzureQueueStorage.Services.Singleton
{
    /// <summary>
    /// Represents an exception that occurs in the context of a Singleton pattern.
    /// </summary>
    public class SingletonException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonException"/> class.
        /// </summary>
        public SingletonException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SingletonException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public SingletonException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

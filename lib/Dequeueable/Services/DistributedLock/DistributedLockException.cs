namespace Dequeueable.Services.DistributedLock
{
    /// <summary>
    /// Represents an exception that occurs in the context of the distributed lock.
    /// </summary>
    public class DistributedLockException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedLockException"/> class.
        /// </summary>
        public DistributedLockException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedLockException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DistributedLockException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedLockException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public DistributedLockException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

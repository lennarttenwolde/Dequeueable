namespace Dequeueable.Configurations
{
    /// <summary>
    /// Interface to builds and setup the dequeueable host
    /// </summary>
    public interface IDequeueableHostBuilder
    {
        /// <summary>
        /// This makes sure only a single instance of the function is executed at any given time (even across host instances).
        /// A blob lease is used behind the scenes to implement the lock./>
        /// </summary>
        /// <param name="options">Action to configure the <see cref="DistributedLockOptions"/></param>
        /// <returns><see cref="IDequeueableHostBuilder"/></returns>
        IDequeueableHostBuilder WithDistributedLock(Action<DistributedLockOptions>? options = null);
    }
}
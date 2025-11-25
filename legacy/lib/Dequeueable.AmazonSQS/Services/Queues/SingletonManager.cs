using System.Collections.Concurrent;

namespace Dequeueable.AmazonSQS.Services.Queues
{
    internal sealed class SingletonManager
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        public Task WaitAsync(string messageGroupId, CancellationToken cancellationToken)
        {
            var _lock = _locks.GetOrAdd(messageGroupId, (_) =>
            {
                return new SemaphoreSlim(1, 1);
            });
            return _lock.WaitAsync(cancellationToken);
        }

        public void Release(string messageGroupId)
        {
            var _lock = _locks.GetValueOrDefault(messageGroupId);

            _lock?.Release();
        }
    }
}

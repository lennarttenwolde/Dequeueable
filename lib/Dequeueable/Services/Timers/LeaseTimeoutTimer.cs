using Dequeueable.AzureQueueStorage.Services.Singleton;

namespace Dequeueable.AzureQueueStorage.Services.Timers
{
    internal sealed class LeaseTimeoutTimer(ISingletonLockManager singletonLockManager, TimeProvider timeProvider, IDelayStrategy delayStrategy) : IAsyncDisposable
    {
        private readonly CancellationTokenSource _cts = new();
        private Task? _backgroundThread;
        private bool _disposed;

        public void Start(string leaseId, string fileName, Action? onFaultedAction = null)
        {
            _backgroundThread = TimerLoop(leaseId, fileName, onFaultedAction);
        }

        private async Task TimerLoop(string leaseId, string fileName, Action? onFaultedAction)
        {
            var leaseExpiresOn = timeProvider.GetUtcNow().Add(delayStrategy.MinimalRenewalDelay);
            using var timer = new PeriodicTimer(delayStrategy.GetNextDelay(leaseExpiresOn), timeProvider);
            while (await timer.WaitForNextTickAsync(_cts.Token))
            {
                try
                {
                    leaseExpiresOn = await singletonLockManager.RenewLockAsync(leaseId, fileName, _cts.Token);
                    timer.Period = delayStrategy.GetNextDelay(leaseExpiresOn);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex) when (ex.InnerException is OperationCanceledException)
                {
                }
                catch
                {
                    onFaultedAction?.Invoke();
                    break;
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            await _cts.CancelAsync();

            try
            {
                if (_backgroundThread is not null)
                {
                    await _backgroundThread;
                }
            }
            catch
            {
                // At this point we really just want to stop extending the visibility timeout
            }

            _cts.Dispose();
        }
    }
}

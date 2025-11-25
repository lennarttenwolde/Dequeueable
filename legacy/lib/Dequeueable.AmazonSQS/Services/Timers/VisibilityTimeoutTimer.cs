using Dequeueable.AmazonSQS.Models;
using Dequeueable.AmazonSQS.Services.Queues;
using Dequeueable.AzureQueueStorage.Services.Timers;

namespace Dequeueable.AmazonSQS.Services.Timers
{
    internal sealed class VisibilityTimeoutTimer(IQueueMessageManager queueMessagesManager, TimeProvider timeProvider, IDelayStrategy delayStrategy) : IAsyncDisposable
    {
        private readonly CancellationTokenSource _cts = new();
        private Task? _backgroundThread;
        private bool _disposed;

        public void Start(Message message, Action? onFaultedAction = null)
        {
            _backgroundThread = TimerLoop(message, onFaultedAction);
        }

        private async Task TimerLoop(Message message, Action? onFaultedAction)
        {
            using var timer = new PeriodicTimer(delayStrategy.GetNextDelay(message.NextVisibleOn), timeProvider);
            while (await timer.WaitForNextTickAsync(_cts.Token))
            {
                try
                {
                    var nextVisibleOn = await queueMessagesManager.UpdateVisibilityTimeOutAsync(message, _cts.Token);
                    timer.Period = delayStrategy.GetNextDelay(nextVisibleOn);
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

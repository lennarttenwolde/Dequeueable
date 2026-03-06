namespace Dequeueable.AzureQueueStorage.Services.Timers
{
    internal sealed class LinearDelayStrategy(TimeSpan minimalRenewalDelay) : IDelayStrategy
    {
        public TimeSpan MinimalRenewalDelay { get; set; } = minimalRenewalDelay;
        internal int Divisor { get; set; } = 2;

        public TimeSpan GetNextDelay(DateTimeOffset? nextVisibleOn = null, bool? executionSucceeded = null)
        {
            if (executionSucceeded == false)
            {
                return MinimalRenewalDelay;
            }

            var wait = ((nextVisibleOn?.UtcDateTime ?? DateTimeOffset.UtcNow) - DateTimeOffset.UtcNow) / Divisor;
            return wait.Ticks > 0 ? wait : MinimalRenewalDelay;
        }
    }
}

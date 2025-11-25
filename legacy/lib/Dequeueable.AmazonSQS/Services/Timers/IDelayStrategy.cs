namespace Dequeueable.AzureQueueStorage.Services.Timers
{
    internal interface IDelayStrategy
    {
        TimeSpan MinimalRenewalDelay { get; set; }
        TimeSpan GetNextDelay(DateTimeOffset? nextVisibleOn = null, bool? executionSucceeded = null);
    }
}

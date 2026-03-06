namespace Dequeueable.AzureQueueStorage.Services.Timers
{
    internal sealed class RandomizedExponentialDelayStrategy : IDelayStrategy
    {
        private const int _randomizationFactor = 20;
        private TimeSpan _minimumInterval;
        private readonly TimeSpan _maximumInterval;
        private readonly TimeSpan _deltaBackoff;

        private TimeSpan _currentInterval = TimeSpan.Zero;
        private uint _backoffExponent;
        private Random? _random;

        public RandomizedExponentialDelayStrategy(TimeSpan minimumInterval, TimeSpan maximumInterval, TimeSpan? deltaBackoff = null)
        {
            if (minimumInterval.Ticks < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumInterval), $"'{nameof(minimumInterval)}' must not be negative or zero.");
            }

            if (maximumInterval.Ticks < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumInterval), $"'{nameof(maximumInterval)}' must not be negative or zero.");
            }

            if (minimumInterval.Ticks > maximumInterval.Ticks)
            {
                throw new ArgumentException($"The '{nameof(minimumInterval)}' must not be greater than the '{nameof(maximumInterval)}'.",
                    nameof(minimumInterval));
            }

            _minimumInterval = minimumInterval;
            _maximumInterval = maximumInterval;
            _deltaBackoff = deltaBackoff ?? minimumInterval;
        }

        public TimeSpan MinimalRenewalDelay { get => _minimumInterval; set { _minimumInterval = value; } }

        public TimeSpan GetNextDelay(DateTimeOffset? _ = null, bool? executionSucceeded = null)
        {
            if (executionSucceeded == true)
            {
                _currentInterval = _minimumInterval;
                _backoffExponent = 1;
            }
            else if (_currentInterval != _maximumInterval)
            {
                var backoffInterval = _minimumInterval;

                if (_backoffExponent > 0)
                {
                    _random ??= new Random();
#pragma warning disable CA5394 // Do not use insecure randomness
                    var randomIncrementMsec = (double)_random.Next(100 - _randomizationFactor, 100 + _randomizationFactor) / 100;
#pragma warning restore CA5394 // Do not use insecure randomness
                    var incrementMsec = randomIncrementMsec *
                        Math.Pow(2.0, _backoffExponent - 1) *
                        _deltaBackoff.TotalMilliseconds;
                    backoffInterval += TimeSpan.FromMilliseconds(incrementMsec);
                }

                if (backoffInterval < _maximumInterval)
                {
                    _currentInterval = backoffInterval;
                    _backoffExponent++;
                }
                else
                {
                    _currentInterval = _maximumInterval;
                }
            }

            return _currentInterval;
        }
    }
}

using Dequeueable.Models;

namespace Dequeueable.IntegrationTests.TestDataBuilders
{
#pragma warning disable CA1515 // Consider making public types internal
    public class TestJob : IQueueJob
#pragma warning restore CA1515 // Consider making public types internal
    {
        private readonly IFakeService _fakeService;

        public TestJob(IFakeService fakeService)
        {
            _fakeService = fakeService;
        }

        public Task ExecuteAsync(Message message, CancellationToken cancellationToken)
        {
            return _fakeService.Execute(message);
        }
    }

#pragma warning disable CA1515 // Consider making public types internal
    public interface IFakeService
#pragma warning restore CA1515 // Consider making public types internal
    {
        Task Execute(Message message);
    }
    internal sealed class FakeService(bool shouldThrow = false, TimeSpan? delay = null) : IFakeService
    {
        public List<Message> ExecutedMessages { get; } = [];

        public async Task Execute(Message message)
        {
            if (shouldThrow)
                throw new Exception("Test exception");

            if (delay.HasValue)
                await Task.Delay(delay.Value);

            ExecutedMessages.Add(message);
        }
    }
}

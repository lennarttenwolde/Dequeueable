using Dequeueable.Models;

namespace Dequeueable.IntegrationTests.TestDataBuilders
{
#pragma warning disable CA1515 // Consider making public types internal
    public class TestFunction : IQueueJob
#pragma warning restore CA1515 // Consider making public types internal
    {
        private readonly IFakeService _fakeService;

        public TestFunction(IFakeService fakeService)
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

#pragma warning disable CA1515 // Consider making public types internal
    public class FakeService : IFakeService
#pragma warning restore CA1515 // Consider making public types internal
    {

        public Task Execute(Message message) { return Task.CompletedTask; }
    }
}

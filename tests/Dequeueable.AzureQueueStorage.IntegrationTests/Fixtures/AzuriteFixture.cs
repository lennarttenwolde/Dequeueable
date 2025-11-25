using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using System.Net;
using System.Net.Sockets;

namespace Dequeueable.AzureQueueStorage.IntegrationTests.Fixtures
{
#pragma warning disable CA1515 // Consider making public types internal
    public class AzuriteFixture : IAsyncLifetime
#pragma warning restore CA1515 // Consider making public types internal
    {
        private int? _blobPort;
        private int? _queuePort;
        private int? _tablePort;

        public int BlobPort
        {
            get
            {
                if (!_blobPort.HasValue)
                {
                    _blobPort = GetAvailablePort();
                }

                return _blobPort.Value;
            }
        }

        public int QueuePort
        {
            get
            {
                if (!_queuePort.HasValue)
                {
                    _queuePort = GetAvailablePort();
                }

                return _queuePort.Value;
            }
        }

        public int TablePort
        {
            get
            {
                if (!_tablePort.HasValue)
                {
                    _tablePort = GetAvailablePort();
                }

                return _tablePort.Value;
            }
        }

        public string ConnectionString => $"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:{BlobPort}/devstoreaccount1;QueueEndpoint=http://127.0.0.1:{QueuePort}/devstoreaccount1;TableEndpoint=http://127.0.0.1:{TablePort}/devstoreaccount1;";

        private IContainer ContainerBuilder => new ContainerBuilder()
                 .WithImage("mcr.microsoft.com/azure-storage/azurite")
                 .WithPortBinding(BlobPort, 10000)
                 .WithPortBinding(QueuePort, 10001)
                 .WithPortBinding(TablePort, 10002)
                 .WithCleanUp(true)
                 .WithWaitStrategy(Wait.ForUnixContainer().UntilExternalTcpPortIsAvailable(10001))
                 .Build();

        public async Task InitializeAsync()
        {
            await ContainerBuilder.StartAsync();
        }

        public Task DisposeAsync()
        {
            return ContainerBuilder.DisposeAsync().AsTask();
        }

        private static readonly IPEndPoint _defaultLoopbackEndpoint = new(IPAddress.Loopback, port: 0);
        private static int GetAvailablePort()
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(_defaultLoopbackEndpoint);
                return ((IPEndPoint)socket.LocalEndPoint!).Port;
            }
        }
    }
}

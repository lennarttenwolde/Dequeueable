using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using System.Net;
using System.Net.Sockets;

namespace Dequeueable.AmazonSQS.IntegrationTests.Fixtures
{
    public class LocalStackFixture : IAsyncLifetime
    {
        private int? _port;

        public int Port
        {
            get
            {
                if (!_port.HasValue)
                {
                    _port = GetAvailablePort();
                }

                return _port.Value;
            }
        }

        public string SQSURL => $"http://localhost:{Port}";

        private IContainer ContainersBuilder => new ContainerBuilder()
                 .WithImage("localstack/localstack")
                 .WithPortBinding(Port, 4566)
                 .WithCleanUp(true)
                 .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(4566))
                 .Build();

        public async Task InitializeAsync()
        {
            await ContainersBuilder.StartAsync();
        }

        public Task DisposeAsync()
        {
            return ContainersBuilder.DisposeAsync().AsTask();
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

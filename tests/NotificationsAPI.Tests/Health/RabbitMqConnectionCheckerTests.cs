using Microsoft.Extensions.Options;
using NotificationsAPI.Health;
using NotificationsAPI.Options;
using Shouldly;
using System.Net;
using System.Net.Sockets;

namespace NotificationsAPI.Tests.Health
{
    public sealed class RabbitMqConnectionCheckerTests
    {
        [Fact]
        public async Task CanConnectAsync_WhenTcpPortIsAcceptingConnections_ReturnsTrue()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);

            try
            {
                listener.Start();
                var endpoint = (IPEndPoint)listener.LocalEndpoint;
                var checker = new RabbitMqConnectionChecker(Microsoft.Extensions.Options.Options.Create(new RabbitMqOptions
                {
                    Host = IPAddress.Loopback.ToString(),
                    Port = endpoint.Port
                }));

                var acceptTask = listener.AcceptTcpClientAsync();

                var result = await checker.CanConnectAsync();

                result.ShouldBeTrue();
                using var acceptedClient = await acceptTask.WaitAsync(TimeSpan.FromSeconds(2));
            }
            finally
            {
                listener.Stop();
            }
        }
    }
}

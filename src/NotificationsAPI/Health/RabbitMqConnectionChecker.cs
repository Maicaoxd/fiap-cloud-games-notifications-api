using Microsoft.Extensions.Options;
using NotificationsAPI.Options;
using System.Net.Sockets;

namespace NotificationsAPI.Health
{
    public interface IRabbitMqConnectionChecker
    {
        Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
    }

    public sealed class RabbitMqConnectionChecker : IRabbitMqConnectionChecker
    {
        private readonly RabbitMqOptions _options;

        public RabbitMqConnectionChecker(IOptions<RabbitMqOptions> options)
        {
            _options = options.Value;
        }

        public async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(_options.Host, _options.Port, cancellationToken);
                return client.Connected;
            }
            catch
            {
                return false;
            }
        }
    }
}

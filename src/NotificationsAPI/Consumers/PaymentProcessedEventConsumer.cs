using FiapCloudGames.Contracts.Events;
using MassTransit;

namespace NotificationsAPI.Consumers
{
    public sealed class PaymentProcessedEventConsumer : IConsumer<PaymentProcessedEvent>
    {
        private const string ApprovedStatus = "Approved";

        private readonly ILogger<PaymentProcessedEventConsumer> _logger;

        public PaymentProcessedEventConsumer(ILogger<PaymentProcessedEventConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<PaymentProcessedEvent> context)
        {
            var message = context.Message;

            if (!string.Equals(message.Status, ApprovedStatus, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation(
                    "Notificacao de compra nao enviada. OrderId: {OrderId}, UserId: {UserId}, Status: {Status}",
                    message.OrderId,
                    message.UserId,
                    message.Status);

                return Task.CompletedTask;
            }

            _logger.LogInformation(
                "E-mail de confirmacao de compra enviado. OrderId: {OrderId}, UserId: {UserId}, Jogos: {GameCount}, Total: {TotalPrice}",
                message.OrderId,
                message.UserId,
                message.Games.Count,
                message.TotalPrice);

            return Task.CompletedTask;
        }
    }
}

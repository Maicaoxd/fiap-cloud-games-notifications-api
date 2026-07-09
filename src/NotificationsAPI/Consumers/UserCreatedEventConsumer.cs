using FiapCloudGames.Contracts.Events;
using MassTransit;

namespace NotificationsAPI.Consumers
{
    public sealed class UserCreatedEventConsumer : IConsumer<UserCreatedEvent>
    {
        private readonly ILogger<UserCreatedEventConsumer> _logger;

        public UserCreatedEventConsumer(ILogger<UserCreatedEventConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<UserCreatedEvent> context)
        {
            var message = context.Message;

            _logger.LogInformation(
                "E-mail de boas-vindas enviado para {Name} ({Email}). UserId: {UserId}",
                message.Name,
                message.Email,
                message.UserId);

            return Task.CompletedTask;
        }
    }
}

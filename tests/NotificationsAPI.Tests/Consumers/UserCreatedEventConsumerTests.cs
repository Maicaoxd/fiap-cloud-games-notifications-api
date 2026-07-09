using FiapCloudGames.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationsAPI.Consumers;
using NSubstitute;
using Shouldly;

namespace NotificationsAPI.Tests.Consumers
{
    public sealed class UserCreatedEventConsumerTests
    {
        [Fact]
        public async Task Consume_WhenUserCreatedEventIsReceived_LogsWelcomeEmailSimulation()
        {
            var logger = new TestLogger<UserCreatedEventConsumer>();
            var consumer = new UserCreatedEventConsumer(logger);
            var message = new UserCreatedEvent(
                Guid.NewGuid(),
                "Maicon",
                "maicon@email.com",
                DateTime.UtcNow);

            var context = Substitute.For<ConsumeContext<UserCreatedEvent>>();
            context.Message.Returns(message);

            await consumer.Consume(context);

            logger.Messages.ShouldContain(log =>
                log.Contains("E-mail de boas-vindas enviado para Maicon", StringComparison.OrdinalIgnoreCase) &&
                log.Contains("maicon@email.com", StringComparison.OrdinalIgnoreCase));
        }

        private sealed class TestLogger<T> : ILogger<T>
        {
            public List<string> Messages { get; } = new();

            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull
            {
                return NullScope.Instance;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                Messages.Add(formatter(state, exception));
            }

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();

                public void Dispose()
                {
                }
            }
        }
    }
}

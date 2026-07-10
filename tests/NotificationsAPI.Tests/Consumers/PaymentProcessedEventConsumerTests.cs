using FiapCloudGames.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationsAPI.Consumers;
using NSubstitute;
using Shouldly;

namespace NotificationsAPI.Tests.Consumers
{
    public sealed class PaymentProcessedEventConsumerTests
    {
        [Fact]
        public async Task Consume_WhenPaymentIsApproved_LogsPurchaseConfirmationEmailSimulation()
        {
            var logger = new TestLogger<PaymentProcessedEventConsumer>();
            var consumer = new PaymentProcessedEventConsumer(logger);
            var message = CreateEvent("Approved");
            var context = Substitute.For<ConsumeContext<PaymentProcessedEvent>>();
            context.Message.Returns(message);

            await consumer.Consume(context);

            logger.Messages.ShouldContain(log =>
                log.Contains("E-mail de confirmacao de compra enviado", StringComparison.OrdinalIgnoreCase) &&
                log.Contains(message.OrderId.ToString(), StringComparison.OrdinalIgnoreCase) &&
                log.Contains(message.UserId.ToString(), StringComparison.OrdinalIgnoreCase) &&
                log.Contains("Jogos: 2", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Consume_WhenPaymentIsRejected_DoesNotLogPurchaseConfirmationEmailSimulation()
        {
            var logger = new TestLogger<PaymentProcessedEventConsumer>();
            var consumer = new PaymentProcessedEventConsumer(logger);
            var message = CreateEvent("Rejected");
            var context = Substitute.For<ConsumeContext<PaymentProcessedEvent>>();
            context.Message.Returns(message);

            await consumer.Consume(context);

            logger.Messages.ShouldNotContain(log =>
                log.Contains("E-mail de confirmacao de compra enviado", StringComparison.OrdinalIgnoreCase));
            logger.Messages.ShouldContain(log =>
                log.Contains("Notificacao de compra nao enviada", StringComparison.OrdinalIgnoreCase) &&
                log.Contains("Rejected", StringComparison.OrdinalIgnoreCase));
        }

        private static PaymentProcessedEvent CreateEvent(string status)
        {
            return new PaymentProcessedEvent(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new[]
                {
                    new PaymentProcessedGameEventItem(Guid.NewGuid(), 49.90m),
                    new PaymentProcessedGameEventItem(Guid.NewGuid(), 24.90m)
                },
                74.80m,
                status,
                DateTime.UtcNow);
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

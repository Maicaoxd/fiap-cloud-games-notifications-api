using MassTransit;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using NotificationsAPI.Consumers;
using NotificationsAPI.Health;
using NotificationsAPI.Options;

namespace NotificationsAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiPresentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "FIAP Cloud Games Notifications API",
                    Version = "v1",
                    Description = "Microservico responsavel por simular notificacoes por e-mail via eventos."
                });
            });

            return services;
        }

        public static IServiceCollection AddMessaging(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddOptions<RabbitMqOptions>()
                .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
                .Validate(options => !string.IsNullOrWhiteSpace(options.Host), "RabbitMq:Host é obrigatório.")
                .Validate(options => options.Port is > 0 and <= 65535, "RabbitMq:Port deve estar entre 1 e 65535.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.VirtualHost), "RabbitMq:VirtualHost é obrigatório.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.Username), "RabbitMq:Username é obrigatório.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.Password), "RabbitMq:Password é obrigatório.")
                .ValidateOnStart();

            services.AddSingleton<IRabbitMqConnectionChecker, RabbitMqConnectionChecker>();

            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumer<UserCreatedEventConsumer>()
                    .Endpoint(endpoint => endpoint.Name = "notifications-user-created-event");
                x.AddConsumer<PaymentProcessedEventConsumer>()
                    .Endpoint(endpoint => endpoint.Name = "notifications-payment-processed-event");

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqOptions = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                    var virtualHostPath = rabbitMqOptions.VirtualHost == "/"
                        ? string.Empty
                        : Uri.EscapeDataString(rabbitMqOptions.VirtualHost.TrimStart('/'));

                    var hostAddress = new UriBuilder("rabbitmq", rabbitMqOptions.Host, rabbitMqOptions.Port, virtualHostPath).Uri;

                    cfg.Host(hostAddress, h =>
                    {
                        h.Username(rabbitMqOptions.Username);
                        h.Password(rabbitMqOptions.Password);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}

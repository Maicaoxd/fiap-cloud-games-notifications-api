using NotificationsAPI.Health;

namespace NotificationsAPI.Extensions
{
    public static class WebApplicationExtensions
    {
        public static WebApplication UseApiPresentation(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Notifications API v1");
            });

            app.MapGet("/", () => Results.Redirect("/swagger"))
                .ExcludeFromDescription();

            app.MapGet("/health/live", () => Results.Ok(new
            {
                status = "Healthy",
                service = "NotificationsAPI"
            }))
            .WithName("LiveHealthCheck");

            app.MapGet("/health", CheckReadinessAsync)
                .WithName("HealthCheck");

            app.MapGet("/health/ready", CheckReadinessAsync)
                .WithName("ReadyHealthCheck");

            return app;
        }

        private static async Task<IResult> CheckReadinessAsync(
            IRabbitMqConnectionChecker rabbitMqConnectionChecker,
            CancellationToken cancellationToken)
        {
            var canConnectToRabbitMq = await rabbitMqConnectionChecker.CanConnectAsync(cancellationToken);

            if (!canConnectToRabbitMq)
            {
                return Results.Json(new
                {
                    status = "Unhealthy",
                    service = "NotificationsAPI",
                    checks = new
                    {
                        rabbitMq = "Unhealthy"
                    }
                }, statusCode: StatusCodes.Status503ServiceUnavailable);
            }

            return Results.Ok(new
            {
                status = "Healthy",
                service = "NotificationsAPI",
                checks = new
                {
                    rabbitMq = "Healthy"
                }
            });
        }
    }
}

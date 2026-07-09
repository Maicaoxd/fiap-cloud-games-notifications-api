using NotificationsAPI.Options;
using Shouldly;

namespace NotificationsAPI.Tests.Options
{
    public sealed class RabbitMqOptionsTests
    {
        [Fact]
        public void Constructor_WhenNoConfigurationIsProvided_UsesDevelopmentDefaults()
        {
            var options = new RabbitMqOptions();

            options.Host.ShouldBe("localhost");
            options.Port.ShouldBe(5672);
            options.VirtualHost.ShouldBe("/");
            options.Username.ShouldBe("guest");
            options.Password.ShouldBe("guest");
        }
    }
}

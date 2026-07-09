using NotificationsAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiPresentation();
builder.Services.AddMessaging(builder.Configuration);

var app = builder.Build();

app.UseApiPresentation();

app.Run();

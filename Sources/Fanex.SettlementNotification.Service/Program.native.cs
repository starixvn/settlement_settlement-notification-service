using Fanex.SettlementNotification.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SettlementNotification.Models;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.Configure<AppSettingsKey>(builder.Configuration)
    .AddSingleton(sp => sp.GetRequiredService<IOptions<AppSettingsKey>>().Value);

services.ConfigInternalService(builder.Configuration);

services.AddHostedService<SenderNotificationService>();

var app = builder.Build();

app.MapGet("/", async context =>
{
    await context.Response.WriteAsync("Fanex.SettlementNotification.Service");
});

#if DEBUG
app.Run();
#else
await app.RunAsync("http://0.0.0.0:5000");
#endif
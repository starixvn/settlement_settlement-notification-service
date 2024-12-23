using Fanex.Data.SqlServer;
using Fanex.Logging.Sentry.RavenClient;
using Fanex.SettlementNotification.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ONEbook.UM.DependencyInjection;
using SettlementNotification.Models;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.Configure<AppSettingsKey>(builder.Configuration)
    .AddSingleton(sp => sp.GetRequiredService<IOptions<AppSettingsKey>>().Value);

services.AddOneBookUm(builder.Configuration);

services.AddFanexLogSentry(builder.Configuration);

services.AddFanexSql(builder.Configuration);

services.ConfigInternalService(builder.Configuration);

services.AddConfigRedisCache(builder.Configuration);

services.AddHostedService<SenderNotificationService>();
services.AddHostedService<ConfirmRefundNotificationService>();

var app = builder.Build();

app.Run(async context =>
{
    await context.Response.WriteAsync("OK");
});

await app.RunAsync();
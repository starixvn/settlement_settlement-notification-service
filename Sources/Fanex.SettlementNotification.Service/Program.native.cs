using Fanex.Data.SqlServer;
using Fanex.Logging.Sentry.RavenClient;
using Fanex.SettlementNotification.Service;
using Fanex.SiteDbConnectionManager.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Notification.MoveTickets;
using ONEbook.UM.DependencyInjection;
using Quartz;
using SettlementNotification.Models;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.Configure<AppSettingsKey>(builder.Configuration)
    .AddSingleton(sp => sp.GetRequiredService<IOptions<AppSettingsKey>>().Value);

services.AddSiteDbConnectionManager();

services.AddOneBookUmForService(builder.Configuration);

services.AddFanexLogSentry(builder.Configuration);

services.AddFanexSql(builder.Configuration);

services.ConfigInternalService(builder.Configuration);

services.AddConfigRedisCache(builder.Configuration);

services.AddMoveTickets(builder.Configuration);

services.AddHostedService<SenderNotificationService>();

services.AddHostedService<ConfirmRefundNotificationService>();

services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

services.AddHealthChecks();

var app = builder.Build();
app.MapHealthChecks("/healthz");

await app.RunAsync();
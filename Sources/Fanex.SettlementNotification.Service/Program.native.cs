using Fanex.Data.SqlServer;
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
builder.Services.AddFanexSql(builder.Configuration);

services.AddHostedService<SenderNotificationService>();
services.AddHostedService<ConfirmRefundNotificationService>();

var app = builder.Build();

app.Run(async context =>
{
    await context.Response.WriteAsync("OK");
});

app.Run();
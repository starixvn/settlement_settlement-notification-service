using System.Threading;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using Microsoft.Extensions.Hosting;
using SettlementNotification.Models;
using SettlementNotification.Services;
using StackExchange.Redis;

namespace Fanex.SettlementNotification.Service;

public class ConfirmRefundNotificationService : IHostedService
{
    private readonly IConnectionMultiplexer connectionMultiplexer;
    private readonly ConfirmRefundService confirmRefundService;

    private const string ExpiredKeysChannel = "__keyspace@13__:*";

    public ConfirmRefundNotificationService(
        IConnectionMultiplexer connectionMultiplexer,
        ConfirmRefundService confirmRefundService)
    {
        this.connectionMultiplexer = connectionMultiplexer;
        this.confirmRefundService = confirmRefundService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var subscriber = connectionMultiplexer.GetSubscriber();
        await subscriber.SubscribeAsync(ExpiredKeysChannel, async (channel, value) =>
        {
            if (value != "expired") return;

            var key = GetKey(channel);
            var keyInfo = new ConfirmRefundKeyInfo(key);

            if (!keyInfo.IsValid) return;

            await confirmRefundService.HandleSend(keyInfo);
        });
    }

    private string GetKey(string channel)
    {
        var index = channel.IndexOf(':');

        if (index >= 0 && index < channel.Length - 1)
        {
            return channel[(index + 1)..];
        }
        return channel;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
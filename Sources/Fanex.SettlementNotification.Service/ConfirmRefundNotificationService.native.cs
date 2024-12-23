using Microsoft.Extensions.Hosting;
using SettlementNotification.Models;
using SettlementNotification.Services;
using StackExchange.Redis;
using System.Threading;
using System.Threading.Tasks;

namespace Fanex.SettlementNotification.Service;

public class ConfirmRefundNotificationService(
    IConnectionMultiplexer connectionMultiplexer,
    ConfirmRefundService confirmRefundService)
    : BackgroundService
{
    private const string ExpiredKeysChannel = "__keyspace@13__:*";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = connectionMultiplexer.GetSubscriber();

        await subscriber.SubscribeAsync(ExpiredKeysChannel, Handler);
    }

    private async void Handler(RedisChannel channel, RedisValue value)
    {
        if (value != "expired") return;

        var key = GetKey(channel);
        var keyInfo = new ConfirmRefundKeyInfo(key);

        if (!keyInfo.IsValid) return;

        await confirmRefundService.HandleSend(keyInfo);
    }

    private static string GetKey(string channel)
    {
        var index = channel.IndexOf(':');

        if (index >= 0 && index < channel.Length - 1)
        {
            return channel[(index + 1)..];
        }
        return channel;
    }
}
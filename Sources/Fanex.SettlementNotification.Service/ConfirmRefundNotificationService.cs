using System.Threading;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using Microsoft.Extensions.Hosting;
using SettlementNotification.Constants;
using SettlementNotification.Criteria;
using SettlementNotification.Models;
using SettlementNotification.Services;
using StackExchange.Redis;
using Log = Fanex.Logging.LogService;

namespace Fanex.SettlementNotification.Service;

public class ConfirmRefundNotificationService : IHostedService
{
    private readonly IConnectionMultiplexer connectionMultiplexer;
    private readonly IDynamicRepository repository;
    private readonly NotificationService notificationService;
    private readonly MatchService matchService;

    private const string ExpiredKeysChannel = "__keyspace@13__:*";

    public ConfirmRefundNotificationService(
        IConnectionMultiplexer connectionMultiplexer,
        IDynamicRepository repository,
        NotificationService notificationService,
        MatchService matchService)
    {
        this.connectionMultiplexer = connectionMultiplexer;
        this.repository = repository;
        this.notificationService = notificationService;
        this.matchService = matchService;
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

            var matchDetail = await matchService.GetMatchDetails(keyInfo.MatchId);

            var resultDetail = await matchService.GetResultDetail(keyInfo.MatchId, keyInfo.ResourceId);

            var IsCanSendMessage = await CanSendMessage(matchDetail, resultDetail, keyInfo);

            if (!IsCanSendMessage)
            {
                await Log.InfoAsync("Can't send notification for confirm refund", matchDetail, resultDetail);
                return;
            }

            await InsertAndSendNotify(matchDetail, keyInfo);
        });
    }

    private async Task InsertAndSendNotify(MatchDetail matchDetail, ConfirmRefundKeyInfo keyInfo)
    {
        var messageContent = GetMessageContent(keyInfo, matchDetail);

        // insert
        var criteria = new AddNotificationCriteria
        {
            MatchId = keyInfo.MatchId,
            ActionName = "Confirmed Refund All",
            MessageGroupId = 3,
            StatusId = 255,
            MessageContent = messageContent
        };
        await repository.ExecuteAsync(criteria);

        // send message

        var notificationModel = new NotificationModel
        {
            ActionName = "Confirmed Refund All",
            MatchId = keyInfo.MatchId,
            HomeId = matchDetail.HomeId,
            AwayId = matchDetail.AwayId,
            LeagueId = matchDetail.LeagueId,
            MessageContent = messageContent,
            KickOffTime = matchDetail.KickOffTime,
            MessageId = criteria.MessageId,
        };

        notificationService.Notify(notificationModel);
    }

    private async Task<bool> CanSendMessage(MatchDetail matchDetail, ResultDetail resultDetail, ConfirmRefundKeyInfo keyInfo)
    {
        if (matchDetail.SportType == SportType.Soccer
               && (!matchDetail.RemarkSettings.IsConfirmRefundAll
               || !matchDetail.BetStatus.ToLowerInvariant().Contains("refund")))
        {
            return false;
        }

        if (matchDetail.SportType != SportType.Soccer
            && (!resultDetail.ResultDetailSetting.IsConfirmRefund
                && !resultDetail.ResultDetailSetting.IsConfirmedRefundSdt))
        {
            return false;
        }

        var IsExistRunningResultTicket = await matchService.CheckRunningTicket(matchDetail.MatchId, keyInfo.ResourceId);

        return IsExistRunningResultTicket;
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

    private string GetMessageContent(ConfirmRefundKeyInfo keyInfo, MatchDetail matchDetail)
    {
        var LT_1 = "The match has been Confirmed Refund All but has not yet been refunded.";
        var LT_3 = "The result {0} has been confirmed refund at but has not yet been refunded.";

        if (matchDetail.SportType == SportType.Soccer)
        {
            return LT_1;
        }

        return string.Format(LT_3, keyInfo.ResourceId);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
using SettlementNotification.Constants;
using SettlementNotification.Criteria;
using SettlementNotification.Models;
using System.Threading.Tasks;

namespace SettlementNotification.Services;

public interface ConfirmRefundService
{
    Task HandleSend(ConfirmRefundKeyInfo keyInfo);
}

public class ConfirmRefundServiceImp(
    IDynamicRepository repository,
    NotificationService notificationService,
    MatchService matchService)
    : ConfirmRefundService
{
    public async Task HandleSend(ConfirmRefundKeyInfo keyInfo)
    {
        var matchDetail = await matchService.GetMatchDetails(keyInfo.MatchId);

        var resultDetail = await matchService.GetResultDetail(keyInfo.MatchId, keyInfo.ResourceId);

        var isCanSendMessage = await CanSendMessage(matchDetail, resultDetail, keyInfo);

        if (isCanSendMessage)
        {
            await InsertAndSendNotify(matchDetail, keyInfo);
        }
    }

    private async Task InsertAndSendNotify(MatchDetail matchDetail, ConfirmRefundKeyInfo keyInfo)
    {
        var messageContent = GetMessageContent(keyInfo, matchDetail);

        var criteria = new AddNotificationCriteria
        {
            MatchId = keyInfo.MatchId,
            ActionName = matchDetail.SportType == SportType.Soccer ? "Confirmed Refund All" : "Confirmed Refund",
            MessageGroupId = MessageGroup.ConfirmRefund,
            StatusId = 255,
            MessageDetail = keyInfo.ResourceId,
            MessageContent = messageContent
        };

        await repository.ExecuteAsync(criteria);

        if (criteria.ErrCode > 0)
        {
            return;
        }

        var notificationModel = new NotificationModel
        {
            ActionName = criteria.ActionName,
            MatchId = keyInfo.MatchId,
            HomeId = matchDetail.HomeId,
            AwayId = matchDetail.AwayId,
            LeagueId = matchDetail.LeagueId,
            MessageContent = messageContent,
            KickOffTime = matchDetail.KickOffTime,
            MessageId = criteria.MessageId,
            MessageType = criteria.MessageType,
            MessageGroupId = criteria.MessageGroupId,
            IsTest = matchDetail.IsTest
        };

        notificationService.Notify(notificationModel);
    }

    private async Task<bool> CanSendMessage(MatchDetail matchDetail, ResultDetail resultDetail, ConfirmRefundKeyInfo keyInfo)
    {
        var isMainSoccer = matchDetail.SportType == SportType.Soccer && keyInfo.ResourceId == "01,00";

        if (isMainSoccer
               && (!matchDetail.RemarkSettings.IsConfirmRefundAll
               || !string.IsNullOrEmpty(matchDetail.BetStatus) && matchDetail.BetStatus.Equals("Refund;")))
        {
            return false;
        }

        if (resultDetail != null && (!resultDetail.ResultDetailSetting.IsConfirmRefund
                && !resultDetail.ResultDetailSetting.IsConfirmedRefundSdt || SettlementStatus.RefundStatuses.Contains(resultDetail.SettlementStatusId)))
        {
            return false;
        }

        var isExistRunningResultTicket = await matchService.CheckRunningTicket(matchDetail.MatchId, matchDetail.SportType, keyInfo.ResourceId);

        return isExistRunningResultTicket;
    }

    private static string GetMessageContent(ConfirmRefundKeyInfo keyInfo, MatchDetail matchDetail)
    {
        const string LT_1 = "The match has been Confirmed Refund All but has not yet been refunded";
        const string LT_3 = "The result {0} has been confirmed refund at but has not yet been refunded";

        return matchDetail.SportType == SportType.Soccer ? LT_1 : string.Format(LT_3, keyInfo.ResourceName);
    }
}
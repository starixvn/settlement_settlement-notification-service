using Microsoft.Extensions.Logging;
using Notification.MoveTickets.Constants;
using Notification.MoveTickets.Models;
using Notification.MoveTickets.Services;
using Quartz;
using SettlementNotification.Constants;
using SettlementNotification.Models;
using SettlementNotification.Services;

namespace Notification.MoveTickets.Jobs;

[DisallowConcurrentExecution]
public class MatchReadyToMoveJob(
    NotificationService notificationService,
    IMatchesReadyToMoveService matchesReadyToMoveService,
    IAutoMoveStatusService autoMoveStatusService,
    INotificationContentService notificationContentService,
    ILogger<MatchReadyToMoveJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("MatchReadyToMoveJob started at {Time}", DateTime.UtcNow);
        try
        {
            await ProcessMatchReadyToMove();
        }
        catch (Exception ex)
        {
            Fanex.Logging.LogService.Error(ex.Message, ex);
        }
    }

    private async Task ProcessMatchReadyToMove()
    {
        var isAutoMoveEnabled = await autoMoveStatusService.GetAutoMoveStatus();
        var eligibleMatches = await matchesReadyToMoveService.GetEligibleMatches();
        Fanex.Logging.LogService.Info($"MatchReadyToMoveJob started at {DateTime.UtcNow} with {string.Join(", ", eligibleMatches.Select(x => x.MatchId))} and setting {isAutoMoveEnabled}");

        if (eligibleMatches.Count == 0)
        {
            SendNotificationNoEligibleMatch();
            return;
        }

        SendNotificationForEligibleMatches(eligibleMatches, isAutoMoveEnabled);
    }

    private void SendNotificationForEligibleMatches(List<Match> matches, bool isAutoMoveEnabled)
    {
        var notificationContent = notificationContentService.CreateNotificationContent(matches, isAutoMoveEnabled);
        var notificationModel = CreateNotificationModel(notificationContent);

        notificationService.Notify(notificationModel);
    }

    private void SendNotificationNoEligibleMatch()
    {
        var model = new NotificationModel
        {
            ActionName = MoveTicketsMessages.ActionNames.AutoMoveTickets,
            MessageContent = MoveTicketsMessages.ContentTemplates.NoMatchEligible,
            MessageType = MoveTicketsMessages.MessageTypes.Reminder,
            MessageGroupId = MessageGroup.MatchReadyToMove,
            LetstalkGroup = LetstalkGroup.Notification
        };

        notificationService.Notify(model);

    }

    private static NotificationModel CreateNotificationModel(string content)
    {
        return new NotificationModel
        {
            ActionName = MoveTicketsMessages.ActionNames.AutoMoveTickets,
            MessageContent = content,
            MessageType = MoveTicketsMessages.MessageTypes.Reminder,
            MessageGroupId = MessageGroup.MatchReadyToMove,
            LetstalkGroup = LetstalkGroup.Notification
        };
    }

}
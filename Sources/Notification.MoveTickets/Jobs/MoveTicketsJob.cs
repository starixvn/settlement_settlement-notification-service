using Microsoft.Extensions.Logging;
using Notification.MoveTickets.Constants;
using Notification.MoveTickets.Models;
using Notification.MoveTickets.Services;
using Quartz;
using SettlementNotification.Constants;
using SettlementNotification.Models;
using SettlementNotification.Services;
using System.Diagnostics;

namespace Notification.MoveTickets.Jobs;

[DisallowConcurrentExecution]
public class MoveTicketsJob(
    IAutoMoveStatusService autoMoveStatusService,
    IMatchesReadyToMoveService matchesReadyToMoveService,
    IMoveTicketService moveTicketService,
    NotificationService notificationService,
    INotificationContentService notificationContentService,
    ILogger<MoveTicketsJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("MoveTicketsJob started at {Time}", DateTime.UtcNow);
        try
        {
            var stopwatch = Stopwatch.StartNew();
            var setting = await autoMoveStatusService.GetAutoMoveStatus();
            Fanex.Logging.LogService.Info($"MoveTicketsJob started at {DateTime.UtcNow} with {setting}");

            if (!setting)
            {
                SendNotificationOffSetting();
                return;
            }

            var matches = await matchesReadyToMoveService.GetEligibleMatches();
            var (successfulMatches, failedMatches) = await ProcessMatches(matches);
            stopwatch.Stop();

            if (failedMatches.Count == matches.Count)
            {
                SendNotifycationNoMatchMoveSucess();
            }
            else
            {
                SendNotificationCompleteMoveTicket((int)stopwatch.ElapsedMilliseconds, successfulMatches, failedMatches);
            }
        }
        catch (Exception ex)
        {
            Fanex.Logging.LogService.Error(ex.Message, ex);
        }
    }


    private async Task<(List<Match> successful, List<Match> failed)> ProcessMatches(List<Match> matches)
    {
        var successfulMatches = new List<Match>();
        var failedMatches = new List<Match>();

        foreach (var match in matches)
        {

            var result = await moveTicketService.ProcessMoveTicket(match);
            if (result)
            {
                successfulMatches.Add(match);
            }
            else
            {
                failedMatches.Add(match);
            }

        }

        return (successfulMatches, failedMatches);
    }


    private void SendNotificationCompleteMoveTicket(int excuteTime, List<Match> matchesSuccess, List<Match> matchesFail)
    {
        var notificationContent = notificationContentService.CreateExecutionSummaryContent(excuteTime, matchesSuccess, matchesFail);
        var notificationModel = new NotificationModel
        {
            ActionName = MoveTicketsMessages.ActionNames.AutoMoveTickets,
            MessageContent = notificationContent,
            MessageType = MoveTicketsMessages.MessageTypes.Reminder,
            MessageGroupId = MessageGroup.MoveTicket,
            LetstalkGroup = LetstalkGroup.Notification
        };

        notificationService.Notify(notificationModel);
    }

    private void SendNotificationOffSetting()
    {
        notificationService.Notify(new NotificationModel
        {
            ActionName = MoveTicketsMessages.ActionNames.AutoMoveTickets,
            MessageContent = MoveTicketsMessages.ContentTemplates.SettingStatusOffTemplate,
            MessageType = MoveTicketsMessages.MessageTypes.Reminder,
            MessageGroupId = MessageGroup.MatchReadyToMove,
        });
    }

    private void SendNotifycationNoMatchMoveSucess()
    {
        notificationService.Notify(new NotificationModel
        {
            ActionName = MoveTicketsMessages.ActionNames.AutoMoveTickets,
            MessageContent = MoveTicketsMessages.ContentTemplates.NoMatchMovie,
            MessageType = MoveTicketsMessages.MessageTypes.Reminder,
            MessageGroupId = MessageGroup.MatchReadyToMove,
        });
    }
}

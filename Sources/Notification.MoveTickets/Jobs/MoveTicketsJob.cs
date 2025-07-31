using Notification.MoveTickets.Constants;
using Notification.MoveTickets.Services;
using Quartz;
using SettlementNotification.Constants;
using SettlementNotification.Models;
using SettlementNotification.Services;

namespace Notification.MoveTickets.Jobs;

[DisallowConcurrentExecution]
public class MoveTicketsJob(
    IAutoMoveStatusService autoMoveStatusService,
    IMatchesReadyToMoveService matchesReadyToMoveService,
    IMoveTicketService moveTicketService,
    NotificationService notificationService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            if (!await autoMoveStatusService.GetAutoMoveStatus())
            {
                SendNotificationOffSetting();
                return;
            }

            var matchMoveSuccess = 0;
            var matches = await matchesReadyToMoveService.GetEligibleMatches();

            foreach (var item in matches)
            {
                var result = await moveTicketService.ProcessMoveTicket(item);
                matchMoveSuccess = result ? matchMoveSuccess + 1 : matchMoveSuccess;
            }

            if (matchMoveSuccess == 0)
            {
                SendNotifycationNoMatchMoveSucess();
            }
        }
        catch (Exception ex)
        {
            Fanex.Logging.LogService.Error(ex.Message, ex);
        }
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

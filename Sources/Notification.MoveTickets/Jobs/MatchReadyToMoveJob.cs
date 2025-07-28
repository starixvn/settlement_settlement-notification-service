using Fanex.Caching;
using Microsoft.Extensions.Logging;
using Notification.MoveTickets.Constants;
using Notification.MoveTickets.Features.GetMatchesReadyToMove;
using Notification.MoveTickets.Features.GetMatchTicketInfo;
using Notification.MoveTickets.Services;
using Quartz;
using SettlementNotification.Constants;
using SettlementNotification.Models;
using SettlementNotification.Services;

namespace Notification.MoveTickets.Jobs;

[DisallowConcurrentExecution]
public class MatchReadyToMoveJob(
    ILogger<MatchReadyToMoveJob> logger,
    NotificationService notificationService,
    IGetMatchesReadyToMoveHandler getMatchesHandler,
    IGetMatchTicketInfoHandler getMatchTicketInfoHandler,
    ICacheService cacheService,
    INotificationContentService notificationContentService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await ProcessMatchReadyToMove();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred in MatchReadyToMoveJob");
            throw;
        }
    }

    private async Task ProcessMatchReadyToMove()
    {
        var isAutoMoveEnabled = await GetAutoMoveStatus();
        var eligibleMatches = await GetEligibleMatches();

        if (eligibleMatches.Count == 0)
        {
            return;
        }

        await SendNotificationForEligibleMatches(eligibleMatches, isAutoMoveEnabled);
    }

    private async Task SendNotificationForEligibleMatches(List<Match> matches, bool isAutoMoveEnabled)
    {
        var notificationContent = notificationContentService.CreateNotificationContent(matches, isAutoMoveEnabled);
        var notificationModel = CreateNotificationModel(notificationContent);

        notificationService.Notify(notificationModel);
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

    private async Task<List<Match>> GetEligibleMatches()
    {
        var allMatches = await getMatchesHandler.HandleAsync();
        var matchIds = allMatches.Select(m => m.MatchId).ToList();
        var ticketInfos = await getMatchTicketInfoHandler.HandleAsync(matchIds);

        return allMatches
            .Where(match => HasSettledTickets(match.MatchId, ticketInfos))
            .ToList();
    }

    private static bool HasSettledTickets(int matchId, List<TicketInfo> ticketInfos)
    {
        return ticketInfos.Exists(ticket => ticket.MatchId == matchId && ticket.IsExistsSettledTicket);
    }

    private async Task<bool> GetAutoMoveStatus()
    {
        try
        {
            var keyExists = await cacheService.ContainAsync(MoveTicketsMessages.CacheKeys.AutoMoveStatus);
            if (!keyExists)
            {
                return false;
            }

            var (success, value) = await cacheService.TryGetAsync<bool>(MoveTicketsMessages.CacheKeys.AutoMoveStatus);
            return success && value;
        }
        catch
        {
            return false;
        }
    }
}
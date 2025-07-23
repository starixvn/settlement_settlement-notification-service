using Fanex.Caching;
using Microsoft.Extensions.Logging;
using Notification.MoveTickets.Features.GetMatchesReadyToMove;
using Notification.MoveTickets.Features.GetMatchTicketInfo;
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
    ICacheService cacheService) : IJob
{
    private const string AutoMoveStatusKey = "Settlement-AutoMoveStatus";

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("MatchReadyToMoveJob started at {Time}", DateTimeOffset.Now);

        try
        {
            var isAutoMoveEnabled = await GetAutoMoveStatus();
            var matches = await GetEligibleMatches();

            if (matches.Count == 0)
            {
                logger.LogInformation("No eligible matches found for moving tickets");
                return;
            }

            InsertAndSendNotify(matches, isAutoMoveEnabled);

            logger.LogInformation("MatchReadyToMoveJob completed successfully at {Time}", DateTimeOffset.Now);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred in MatchReadyToMoveJob");
            throw;
        }
    }

    private void InsertAndSendNotify(List<Match> matches, bool isAutoMoveEnabled)
    {
        var content = CreateContent(matches, isAutoMoveEnabled);

        var notificationModel = new NotificationModel
        {
            ActionName = "Auto move tickets of settled results",
            MessageContent = content,
            MessageType = "Reminder",
            MessageGroupId = MessageGroup.MatchReadyToMove,
            LetstalkGroup = LetstalkGroup.Notification
        };

        notificationService.Notify(notificationModel);
    }

    private async Task<List<Match>> GetEligibleMatches()
    {
        var matches = await getMatchesHandler.HandleAsync();
        var tickets = await getMatchTicketInfoHandler.HandleAsync([.. matches.Select(m => m.MatchId)]);

        return [.. matches.Where(m => tickets.Exists(t => t.MatchId == m.MatchId && t.IsExistsSettledTicket))];
    }

    private static List<Match> GetOnHoldMatches(List<Match> eligibleMatches)
        => [.. eligibleMatches.Where(m => m.Remark.IsPending || m.Remark.IsUndecided || m.Remark.IsUnderSettle || m.Remark.IsMtdHold || m.Remark.IsKeyHold)];

    private static List<Match> GetMatchesWithoutChildMatches(List<Match> eligibleMatches)
        => [.. eligibleMatches.Where(m => m.ChildMatchId == null || m.ChildMatchId == 0)];

    private static string CreateContent(List<Match> matches, bool isAutoMoveEnabled)
    {
        var baseballMatches = matches.Where(m => m.SportType == SportType.Baseball).ToList();
        var cricketMatches = matches.Where(m => m.SportType == SportType.Cricket).ToList();
        var onHoldMatches = GetOnHoldMatches(matches);
        var matchesWithoutChild = GetMatchesWithoutChildMatches(matches);

        return BuildContent(isAutoMoveEnabled, baseballMatches, cricketMatches, onHoldMatches, matchesWithoutChild);
    }

    private static string BuildContent(bool isAutoMoveEnabled, List<Match> baseballMatches, List<Match> cricketMatches, List<Match> onHoldMatches, List<Match> matchesWithoutChild)
    {
        var content = new List<string>
        {
            $"Auto move tickets of settled results service = {(isAutoMoveEnabled ? "ON" : "OFF")}.\n",
            "System detect following matches eligible for moving tickets of settled results:\n"
        };

        if (baseballMatches.Count > 0)
        {
            content.Add($"+ Baseball: {string.Join(", ", baseballMatches.Select(x => x.MatchId))}\n");
        }

        if (cricketMatches.Count > 0)
        {
            content.Add($"+ Cricket: {string.Join(", ", cricketMatches.Select(x => x.MatchId))}\n");
        }

        content.Add("\nMatches need resolve:\n");

        if (onHoldMatches.Count > 0)
        {
            content.Add($"+ matches on-hold: {string.Join(", ", onHoldMatches.Select(x => x.MatchId))}\n");
        }

        if (matchesWithoutChild.Count > 0)
        {
            content.Add($"+ matches without corresponding child-match to move tickets: {string.Join(", ", matchesWithoutChild.Select(x => x.MatchId))}\n");
        }

        content.AddRange(new[]
        {
            $"\nSystem shall execute move actions in the next 15 minutes (23:45 GMT-4).\n",
            "Unresolved matches will not be moved.\n",
            "System shall also move ticket on new match found that eligible to move tickets."
        });

        return string.Join("", content);
    }

    private async Task<bool> GetAutoMoveStatus()
    {
        try
        {
            var isExistedKey = await cacheService.ContainAsync(AutoMoveStatusKey);
            if (isExistedKey)
            {
                var (_, value) = await cacheService.TryGetAsync<bool>(AutoMoveStatusKey);
                return value;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}

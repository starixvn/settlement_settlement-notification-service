using Fanex.Data.Repository;
using Microsoft.Extensions.Logging;
using Quartz;
using SettlementNotification.Constants;
using SettlementNotification.Criteria;
using SettlementNotification.Models;
using SettlementNotification.Services;
using Notification.MoveTickets.Features.GetMatchesReadyToMove;
using Notification.MoveTickets.Models;

namespace Notification.MoveTickets.Jobs;

[DisallowConcurrentExecution]
public class MatchReadyToMoveJob : IJob
{
    private readonly ILogger<MatchReadyToMoveJob> logger;
    private readonly IDynamicRepository repository;
    private readonly NotificationService notificationService;
    private readonly MatchService matchService;
    private readonly GetMatchesReadyToMoveHandler getMatchesHandler;

    public MatchReadyToMoveJob(
        ILogger<MatchReadyToMoveJob> logger,
        IDynamicRepository repository,
        NotificationService notificationService,
        MatchService matchService,
        GetMatchesReadyToMoveHandler getMatchesHandler)
    {
        this.logger = logger;
        this.repository = repository;
        this.notificationService = notificationService;
        this.matchService = matchService;
        this.getMatchesHandler = getMatchesHandler;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("MatchReadyToMoveJob started at {Time}", DateTimeOffset.Now);

        try
        {
            // Get eligible matches for moving tickets
            var eligibleMatches = await getMatchesHandler.HandleAsync();

            if (!eligibleMatches.Any())
            {
                logger.LogInformation("No eligible matches found for moving tickets");
                return;
            }

            // Separate matches by sport type
            var baseballMatches = eligibleMatches.Where(m => m.SportType == SportType.Baseball).ToList();
            var cricketMatches = eligibleMatches.Where(m => m.SportType == SportType.Cricket).ToList();

            // Check for on-hold matches
            var onHoldMatches = await GetOnHoldMatches(eligibleMatches);

            // Check for matches without child matches
            var matchesWithoutChild = await GetMatchesWithoutChildMatches(eligibleMatches);

            // Create notification content
            var content = CreateContent(new
            {
                BaseballMatches = baseballMatches,
                CricketMatches = cricketMatches,
                OnHoldMatches = onHoldMatches,
                MatchesWithoutChild = matchesWithoutChild,
                ExecutionTime = DateTimeOffset.Now.AddMinutes(15).ToString("HH:mm") + " GMT-4"
            });

            // Send notification
            var notificationModel = new NotificationModel
            {
                MatchId = 0, // System notification, not tied to specific match
                ActionName = "Auto move tickets of settled results",
                MessageGroupId = MessageGroup.MatchReadyToMove,
                MessageContent = content
            };

            notificationService.Notify(notificationModel);

            logger.LogInformation("MatchReadyToMoveJob completed successfully. Eligible matches: {BaseballCount} baseball, {CricketCount} cricket",
                baseballMatches.Count, cricketMatches.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred in MatchReadyToMoveJob");
            throw;
        }
    }

    private async Task<List<Match>> GetOnHoldMatches(List<Match> eligibleMatches)
    {
        var onHoldMatches = new List<Match>();

        foreach (var match in eligibleMatches)
        {
            var matchDetail = await matchService.GetMatchDetails(match.MatchId);
            if (matchDetail?.EventStatus == "OnHold")
            {
                onHoldMatches.Add(match);
            }
        }

        return onHoldMatches;
    }

    private async Task<List<Match>> GetMatchesWithoutChildMatches(List<Match> eligibleMatches)
    {
        var matchesWithoutChild = new List<Match>();

        foreach (var match in eligibleMatches)
        {
            var moveTicketInfo = await GetMoveTicketInfo(match.MatchId);
            if (moveTicketInfo == null || moveTicketInfo.ChildMatchId <= 0)
            {
                matchesWithoutChild.Add(match);
            }
        }

        return matchesWithoutChild;
    }

    private async Task<MoveTicketInfo> GetMoveTicketInfo(int matchId)
    {
        var criteria = new GetMoveTicketInfoCriteria { MatchId = matchId };
        return await repository.GetAsync<MoveTicketInfo>(criteria);
    }

    private string CreateContent(object data)
    {
        var baseballMatches = ((dynamic)data).BaseballMatches as List<Match>;
        var cricketMatches = ((dynamic)data).CricketMatches as List<Match>;
        var onHoldMatches = ((dynamic)data).OnHoldMatches as List<Match>;
        var matchesWithoutChild = ((dynamic)data).MatchesWithoutChild as List<Match>;
        var executionTime = ((dynamic)data).ExecutionTime as string;

        var content = "Auto move tickets of settled results service = @move_setting.\n\n";

        content += "System detect following matches eligible for moving tickets of settled results:\n";

        if (baseballMatches?.Count > 0)
        {
            content += $"+ Baseball: @baseball_match ({baseballMatches.Count} matches)\n";
        }

        if (cricketMatches?.Count > 0)
        {
            content += $"+ Cricket: @cricket_match_to ({cricketMatches.Count} matches)\n";
        }

        if ((baseballMatches?.Count ?? 0) == 0 && (cricketMatches?.Count ?? 0) == 0)
        {
            content += "+ No eligible matches found\n";
        }

        content += "\nMatches need resolve:\n";

        if (onHoldMatches?.Count > 0)
        {
            content += $"+ matches on-hold: @match_onhold ({onHoldMatches.Count} matches)\n";
        }

        if (matchesWithoutChild?.Count > 0)
        {
            content += $"+ matches without corresponding child-match to move tickets: @match_nochild ({matchesWithoutChild.Count} matches)\n";
        }

        if ((onHoldMatches?.Count ?? 0) == 0 && (matchesWithoutChild?.Count ?? 0) == 0)
        {
            content += "+ No matches need resolution\n";
        }

        content += $"\nSystem shall execute move actions in the next 15 minutes ({executionTime}).\n";
        content += "Unresolved matches will not be moved.\n";
        content += "System shall also move ticket on new match found that eligible to move tickets.";

        return content;
    }
}

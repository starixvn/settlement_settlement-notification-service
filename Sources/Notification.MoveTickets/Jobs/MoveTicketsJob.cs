using Fanex.Data.Repository;
using Microsoft.Extensions.Logging;
using Notification.MoveTickets.Features.GetMatchesReadyToMove;
using Notification.MoveTickets.Models;
using Quartz;
using SettlementNotification.Services;

namespace Notification.MoveTickets.Jobs;

[DisallowConcurrentExecution]
public class MoveTicketsJob : IJob
{
    private readonly ILogger<MoveTicketsJob> logger;
    private readonly IDynamicRepository repository;
    private readonly MatchService matchService;
    private readonly GetMatchesReadyToMoveHandler getMatchesHandler;

    public MoveTicketsJob(
        ILogger<MoveTicketsJob> logger,
        IDynamicRepository repository,
        MatchService matchService,
        GetMatchesReadyToMoveHandler getMatchesHandler)
    {
        this.logger = logger;
        this.repository = repository;
        this.matchService = matchService;
        this.getMatchesHandler = getMatchesHandler;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("MoveTicketsJob started at {Time}", DateTimeOffset.Now);

        try
        {
            // Get eligible matches for moving tickets
            var eligibleMatches = await getMatchesHandler.HandleAsync();

            if (!eligibleMatches.Any())
            {
                logger.LogInformation("No eligible matches found for moving tickets");
                return;
            }

            var successCount = 0;
            var errorCount = 0;
            var skippedCount = 0;

            foreach (var match in eligibleMatches)
            {
                try
                {
                    // Check if match is on hold
                    var matchDetail = await matchService.GetMatchDetails(match.MatchId);
                    if (matchDetail?.EventStatus == "OnHold")
                    {
                        logger.LogWarning("Match {MatchId} is on hold, skipping ticket move", match.MatchId);
                        skippedCount++;
                        continue;
                    }

                    // Check if match has child match
                    var moveTicketInfo = await GetMoveTicketInfo(match.MatchId);
                    if (moveTicketInfo == null || moveTicketInfo.ChildMatchId <= 0)
                    {
                        logger.LogWarning("Match {MatchId} has no child match, skipping ticket move", match.MatchId);
                        skippedCount++;
                        continue;
                    }

                    // Execute move ticket operation
                    var result = await MoveTicketsForMatch(match, moveTicketInfo);

                    if (result.ErrCode == 0)
                    {
                        logger.LogInformation("Successfully moved {TicketCount} tickets for match {MatchId}",
                            result.TicketCount, match.MatchId);
                        successCount++;
                    }
                    else
                    {
                        logger.LogError("Failed to move tickets for match {MatchId}: {ErrorMessage}",
                            match.MatchId, result.ErrMsg);
                        errorCount++;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing match {MatchId} for ticket move", match.MatchId);
                    errorCount++;
                }
            }

            logger.LogInformation("MoveTicketsJob completed. Success: {SuccessCount}, Errors: {ErrorCount}, Skipped: {SkippedCount}",
                successCount, errorCount, skippedCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred in MoveTicketsJob");
            throw;
        }
    }

    private async Task<MoveTicketInfo> GetMoveTicketInfo(int matchId)
    {
        var criteria = new GetMoveTicketInfoCriteria { MatchId = matchId };
        return await repository.GetAsync<MoveTicketInfo>(criteria);
    }

    private async Task<ResultMoveTicket> MoveTicketsForMatch(Match match, MoveTicketInfo moveTicketInfo)
    {
        var criteria = new MoveTicketCriteria
        {
            MatchId = match.MatchId,
            AdminId = 1, // System admin ID for auto operations
            Ip = "127.0.0.1", // System IP
            EventDate = moveTicketInfo.EventDate ?? DateTime.Now,
            ChildMatchId = moveTicketInfo.ChildMatchId,
            Resources = moveTicketInfo.Resources,
            TicketCount = 0 // Will be set by stored procedure
        };

        await repository.ExecuteAsync(criteria);

        return new ResultMoveTicket
        {
            ErrCode = criteria.ErrCode,
            ErrMsg = criteria.ErrMsg,
            TicketCount = criteria.TicketCount
        };
    }
}

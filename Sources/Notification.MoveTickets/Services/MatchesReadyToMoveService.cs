using Notification.MoveTickets.Features.GetMatchesReadyToMove;
using Notification.MoveTickets.Features.GetMatchTicketInfo;
using Notification.MoveTickets.Models;

namespace Notification.MoveTickets.Services;

public interface IMatchesReadyToMoveService
{
    Task<List<Match>> GetEligibleMatches();
}

public class MatchesReadyToMoveService(
    IGetMatchesReadyToMoveHandler getMatchesHandler,
    IGetMatchTicketInfoHandler getMatchTicketInfoHandler) : IMatchesReadyToMoveService
{
    public async Task<List<Match>> GetEligibleMatches()
    {
        var allMatches = await getMatchesHandler.HandleAsync();
        Fanex.Logging.LogService.Info($"Found {string.Join(", ", allMatches.Select(x => x.MatchId))} matches ready to move.");
        var ticketInfos = await getMatchTicketInfoHandler.HandleAsync(allMatches);

        return [.. allMatches.Where(match => HasSettledTickets(match.MatchId, ticketInfos))];
    }

    private static bool HasSettledTickets(int matchId, List<TicketInfo> ticketInfos)
        => ticketInfos.Exists(ticket => ticket.MatchId == matchId && ticket.IsExistsTicket);
}

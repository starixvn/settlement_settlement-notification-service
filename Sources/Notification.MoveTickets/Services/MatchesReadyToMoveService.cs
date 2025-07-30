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
        var matchIds = allMatches.Select(m => m.MatchId).ToList();
        var ticketInfos = await getMatchTicketInfoHandler.HandleAsync(matchIds);

        return [.. allMatches.Where(match => HasSettledTickets(match.MatchId, ticketInfos))];
    }

    private static bool HasSettledTickets(int matchId, List<TicketInfo> ticketInfos)
    {
        return ticketInfos.Exists(ticket => ticket.MatchId == matchId && ticket.IsExistsSettledTicket);
    }
}

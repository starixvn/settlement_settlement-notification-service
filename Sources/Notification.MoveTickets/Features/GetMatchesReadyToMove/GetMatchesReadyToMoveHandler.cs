using Fanex.Data.Repository;
using Notification.MoveTickets.Models;

namespace Notification.MoveTickets.Features.GetMatchesReadyToMove;

public interface IGetMatchesReadyToMoveHandler
{
    Task<List<Match>> HandleAsync();
}

public class GetMatchesReadyToMoveHandler(
    IDynamicRepository repository
 ) : IGetMatchesReadyToMoveHandler
{
    public async Task<List<Match>> HandleAsync()
    {
        var criteria = new GetMatchesReadyToMoveCriteria();
        var matches = await repository.FetchAsync<Match>(criteria);

        var newMatches = matches
            .GroupBy(m => m.MatchId)
            .Select(g => new Match
            {
                MatchId = g.Key,
                SportType = g.First().SportType,
                ResourceId = g.First().ResourceId,
                ResourceIds = g.Select(x => x.ResourceId).ToList(),
                SessionTime = g.First().SessionTime,
                GameSession = g.First().GameSession,
                KickOffTime = g.First().KickOffTime,
                ChildMatchId = g.First().ChildMatchId,
                SettlementStatus = g.First().SettlementStatus,
                IsTest = g.First().IsTest,
            }).ToList();

        return [.. newMatches];
    }
}
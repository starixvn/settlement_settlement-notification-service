using Fanex.Data.Repository;

namespace Notification.MoveTickets.Features.GetMoveTicketInfo;

public interface IGetMatchtInfoHandler
{
    Task<MatchMoveInfo> HandleAsync(int matchId);
}

public class GetMatchtInfoHandler(IDynamicRepository repository) : IGetMatchtInfoHandler
{
    public async Task<MatchMoveInfo> HandleAsync(int matchId)
    {

        GetMatchtInfoCriteria criteria = new GetMatchtInfoCriteria
        {
            MatchId = matchId
        };

        return await repository.GetAsync<MatchMoveInfo>(criteria);
    }
}

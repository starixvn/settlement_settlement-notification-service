using Fanex.Data.Repository;
using Fanex.SiteDbConnectionManager.Client.Services;
using Notification.MoveTickets.Features.GetMoveTicketInfo;
using Notification.MoveTickets.Models;

namespace Notification.MoveTickets.Features.MoveTicket;

public interface IMoveTicketHandler
{
    Task HandleAsync(Match match, MatchMoveInfo moveTicketInfo);
}

public class MoveTicketHandler(SiteDbClientService siteDbClientService, IDynamicRepository repository) : IMoveTicketHandler
{
    public async Task HandleAsync(Match match, MatchMoveInfo moveTicketInfo)
    {
        var criteria = new MoveTicketCriteria
        {
            MatchId = match.MatchId,
            EventDate = moveTicketInfo.EventDate,
            ChildMatchId = moveTicketInfo.ChildMatchId,
            Resources = moveTicketInfo.Resources
        };

        var sitesDb = await siteDbClientService.GetActiveSiteDbAsync();
        var tasks = sitesDb.Select(s => MoveTicket(s.SiteName, criteria))
                .ToArray();

        var result = await Task.WhenAll(tasks);

        await UpdateChildMatchInfo(moveTicketInfo.ChildMatchId);


    }

    private async Task<ResultMoveTicket> MoveTicket(string dbServer, MoveTicketCriteria criteria)
    {
        criteria.DbServer = dbServer;
        await repository.ExecuteAsync(criteria);

        return new ResultMoveTicket
        {
            ErrMsg = criteria.ErrMsg,
            ErrCode = criteria.ErrCode,
            TicketCount = criteria.TicketCount
        };
    }

    private async Task UpdateChildMatchInfo(int childMatchId)
    {
        var updateChildMatchCriteria = new UpdateChildMatchCriteria { MatchId = childMatchId };
        await repository.ExecuteAsync(updateChildMatchCriteria);

        var updateChildMatchRemarkCriteria = new UpdateChildMatchRemarkCriteria { MatchId = childMatchId };
        await repository.ExecuteAsync(updateChildMatchRemarkCriteria);
    }
}

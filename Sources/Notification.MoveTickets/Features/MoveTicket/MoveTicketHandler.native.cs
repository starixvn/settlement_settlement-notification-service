using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
using Notification.MoveTickets.Constants;
using Notification.MoveTickets.Features.GetMoveTicketInfo;
using Notification.MoveTickets.Models;
using SettlementNotification.Models;
using SettlementNotification.Services;

namespace Notification.MoveTickets.Features.MoveTicket;

public interface IMoveTicketHandler
{
    Task<(bool, int)> HandleAsync(Match match, MatchMoveInfo moveTicketInfo);
}

public class MoveTicketHandler(
    IDynamicRepository repository,
    ISettleLogService settleLogService,
    IOptions<AppSettingsKey> settingOptions) : IMoveTicketHandler
{
    public async Task<(bool, int)> HandleAsync(Match match, MatchMoveInfo moveTicketInfo)
    {
        var criteria = new MoveTicketCriteria
        {
            MatchId = match.MatchId,
            EventDate = moveTicketInfo.EventDate,
            ChildMatchId = moveTicketInfo.ChildMatchId,
            Resources = moveTicketInfo.Resources
        };

        var tasks = settingOptions.Value.SiteDbServer.Select(s => MoveTicket(s, criteria))
                .ToArray();

        var results = (await Task.WhenAll(tasks))
            .ToList();

        if (results.Exists(x => x.ErrCode > 0))
        {
            await settleLogService.WriteLog(
                match.MatchId,
                $"Move ticket failed: {string.Join(", ", results.Select(r => r.ErrMsg))}",
                MoveTicketConts.ActionIdMoveTicketFailed);

            return (false, 0);
        }

        if (!results.Exists(x => x.TicketCount > 0))
        {
            await settleLogService.WriteLog(
                match.MatchId,
                MoveTicketConts.NotExistValidTicketToMove,
                MoveTicketConts.ActionIdMoveTicketFailed);
            return (false, 0);
        }

        return (true, results.Sum(x => x.TicketCount));
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

}

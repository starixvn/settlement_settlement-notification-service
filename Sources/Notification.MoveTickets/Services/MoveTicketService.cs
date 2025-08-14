using Notification.MoveTickets.Constants;
using Notification.MoveTickets.Features.GetMoveTicketInfo;
using Notification.MoveTickets.Features.MoveTicket;
using Notification.MoveTickets.Features.UpdateMatch;
using Notification.MoveTickets.Features.VerifyBeforeMoveTickets;
using Notification.MoveTickets.Models;
using SettlementNotification.Services;
using System.Diagnostics;

namespace Notification.MoveTickets.Services;

public interface IMoveTicketService
{
    Task<bool> ProcessMoveTicket(Match match);
}

public class MoveTicketService(
    IVerifyBeforeMoveTicketsHandler verifyBeforeMoveTicketsHandler,
    IGetMatchtInfoHandler getMatchtInfoHandler,
    IMoveTicketHandler moveTicketHandler,
    ISettleLogService settleLogService,
    IUpdateMatchHandler updateMatchHandler

    ) : IMoveTicketService
{
    public async Task<bool> ProcessMoveTicket(Match match)
    {
        var result = await verifyBeforeMoveTicketsHandler.HandleAsync(match.MatchId);
        if (result.ErrCode != 0)
        {
            await settleLogService.WriteLog(
                match.MatchId,
                result.ErrMsg,
                MoveTicketConts.ActionIdMoveTicketFailed);

            return false;
        }

        // get tickets that are ready to move
        var moveTicketInfo = await getMatchtInfoHandler.HandleAsync(match.MatchId);

        // move tickets
        var stopWatch = Stopwatch.StartNew();
        var (resultMoveTicket, ticketCount) = await moveTicketHandler.HandleAsync(match, moveTicketInfo);
        stopWatch.Stop();

        if (resultMoveTicket)
        {
            var logMessage = $"{moveTicketInfo.ChildMatchId}<br/> ({moveTicketInfo.Resources})<br/>Duration:{stopWatch.ElapsedMilliseconds}ms; TC:{ticketCount}; IsAuto: 1";
            await settleLogService.WriteLog(
                match.MatchId,
                logMessage,
                MoveTicketConts.ActionIdMoveTicket);

            await updateMatchHandler.HandleAsync(moveTicketInfo.ChildMatchId);
        }

        return false;
    }
}

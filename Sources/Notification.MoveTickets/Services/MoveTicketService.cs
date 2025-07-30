using Notification.MoveTickets.Features.GetMoveTicketInfo;
using Notification.MoveTickets.Features.MoveTicket;
using Notification.MoveTickets.Features.VerifyBeforeMoveTickets;
using Notification.MoveTickets.Models;

namespace Notification.MoveTickets.Services;

public interface IMoveTicketService
{
    Task ProcessMoveTicket(Match match);
}

public class MoveTicketService(
    IVerifyBeforeMoveTicketsHandler verifyBeforeMoveTicketsHandler,
    IGetMatchtInfoHandler getMatchtInfoHandler,
    IMoveTicketHandler moveTicketHandler
    ) : IMoveTicketService
{
    public async Task ProcessMoveTicket(Match match)
    {
        // verify if match is ready to move
        var result = await verifyBeforeMoveTicketsHandler.HandleAsync(match.MatchId);
        if (result.ErrCode != 0)
        {
            throw new InvalidOperationException($"Cannot move tickets for match {match.MatchId}: {result.ErrMsg}");
        }

        // get tickets that are ready to move
        var moveTicketInfo = await getMatchtInfoHandler.HandleAsync(match.MatchId);


        // move tickets

        await moveTicketHandler.HandleAsync(match, moveTicketInfo);
        // update child match info
    }
}

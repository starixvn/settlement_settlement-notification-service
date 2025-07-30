using Notification.MoveTickets.Services;
using Quartz;

namespace Notification.MoveTickets.Jobs;

[DisallowConcurrentExecution]
public class MoveTicketsJob(
    IAutoMoveStatusService autoMoveStatusService,
    IMatchesReadyToMoveService matchesReadyToMoveService,
    IMoveTicketService moveTicketService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        if (!await autoMoveStatusService.GetAutoMoveStatus())
        {
            return;
        }

        var matches = await matchesReadyToMoveService.GetEligibleMatches();

        foreach (var item in matches)
        {
            await moveTicketService.ProcessMoveTicket(item);
        }
    }
}

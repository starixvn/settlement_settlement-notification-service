using Quartz;

namespace Notification.MoveTickets.Jobs;

[DisallowConcurrentExecution]
public class MoveTicketsJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        // get match
        // verify if match is ready to move
        // get tickets that are ready to move
        // move tickets
        // update child match info
    }
}

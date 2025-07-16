using Microsoft.Extensions.Logging;
using Quartz;

namespace Notification.MoveTickets.Jobs;


[DisallowConcurrentExecution]
public class MoveTicketsJob : IJob
{
    private readonly ILogger<MoveTicketsJob> _logger;

    public MoveTicketsJob(ILogger<MoveTicketsJob> logger)
    {
        _logger = logger;
    }
    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("MoveTicketsJob started at {Time}", DateTimeOffset.Now);
    }
}

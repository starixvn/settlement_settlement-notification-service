using Fanex.Data.Repository;
using Microsoft.Extensions.Logging;
using Quartz;
using SettlementNotification.Constants;
using SettlementNotification.Criteria;
using SettlementNotification.Services;

namespace Notification.MoveTickets.Jobs;


[DisallowConcurrentExecution]
public class MatchReadyToMoveJob : IJob
{
    private readonly ILogger<MatchReadyToMoveJob> _logger;
    private readonly IDynamicRepository _repository;
    private readonly NotificationService _notificationService;

    public MatchReadyToMoveJob(
        ILogger<MatchReadyToMoveJob> logger,
        IDynamicRepository repository,
        NotificationService notificationService)
    {
        _logger = logger;
        _repository = repository;
        _notificationService = notificationService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("MatchReadyToMoveJob started at {Time}", DateTimeOffset.Now);
        var addCriteria = new AddNotificationCriteria
        {
            MatchId = 12345, // Example match ID
            ActionName = "Auto move tickets of setled results",
            MessageGroupId = MessageGroup.MatchReadyToMove,
            StatusId = 1, // Example status ID
            MessageDetail = "Details about the match ready to move",
            MessageContent = "Content of the notification for match ready to move"
        };
    }
}

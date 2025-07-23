namespace Notification.MoveTickets.Models;

public class MoveTicketsJobSettings
{
    public JobSchedule MatchReadyToMoveJob { get; set; } = new();
    public JobSchedule MoveTicketsJob { get; set; } = new();
}

public class JobSchedule
{
    public string CronExpression { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
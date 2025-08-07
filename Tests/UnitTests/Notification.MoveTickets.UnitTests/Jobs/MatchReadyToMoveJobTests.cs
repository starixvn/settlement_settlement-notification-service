using Microsoft.Extensions.Logging;
using Notification.MoveTickets.Constants;
using Notification.MoveTickets.Jobs;
using Notification.MoveTickets.Models;
using Notification.MoveTickets.Services;
using NSubstitute;
using Quartz;
using SettlementNotification.Constants;
using SettlementNotification.Models;
using SettlementNotification.Services;
using Xunit;

namespace Notification.MoveTickets.UnitTests.Jobs;

public class MatchReadyToMoveJobTests
{
    private readonly ILogger<MatchReadyToMoveJob> logger;
    private readonly NotificationService notificationService;
    private readonly IMatchesReadyToMoveService matchesReadyToMoveService;
    private readonly IAutoMoveStatusService autoMoveStatusService;
    private readonly INotificationContentService notificationContentService;
    private readonly MatchReadyToMoveJob job;
    private readonly IJobExecutionContext jobContext;

    public MatchReadyToMoveJobTests()
    {
        logger = Substitute.For<ILogger<MatchReadyToMoveJob>>();
        notificationService = Substitute.For<NotificationService>();
        matchesReadyToMoveService = Substitute.For<IMatchesReadyToMoveService>();
        autoMoveStatusService = Substitute.For<IAutoMoveStatusService>();
        notificationContentService = Substitute.For<INotificationContentService>();
        job = new MatchReadyToMoveJob(notificationService, matchesReadyToMoveService, autoMoveStatusService, notificationContentService);
        jobContext = Substitute.For<IJobExecutionContext>();
    }

    [Fact]
    public async Task Execute_WithNoEligibleMatches_SendsNoEligibleMatchNotification()
    {
        // Arrange
        autoMoveStatusService.GetAutoMoveStatus().Returns(true);
        matchesReadyToMoveService.GetEligibleMatches().Returns(new List<Match>());

        // Act
        await job.Execute(jobContext);

        // Assert
        notificationService.Received(1).Notify(Arg.Is<NotificationModel>(n =>
            n.ActionName == MoveTicketsMessages.ActionNames.AutoMoveTickets &&
            n.MessageContent == MoveTicketsMessages.ContentTemplates.NoMatchEligible &&
            n.MessageType == MoveTicketsMessages.MessageTypes.Reminder &&
            n.MessageGroupId == MessageGroup.MatchReadyToMove &&
            n.LetstalkGroup == LetstalkGroup.Notification
        ));
    }

    [Fact]
    public async Task Execute_WithEligibleMatches_SendsNotification()
    {
        // Arrange
        autoMoveStatusService.GetAutoMoveStatus().Returns(true);
        var matches = new List<Match> { new Match { MatchId = 1 } };
        matchesReadyToMoveService.GetEligibleMatches().Returns(matches);
        notificationContentService.CreateNotificationContent(matches, true).Returns("content");

        // Act
        await job.Execute(jobContext);

        // Assert
        notificationService.Received(1).Notify(Arg.Is<NotificationModel>(n =>
            n.MessageContent == "content" &&
            n.ActionName == MoveTicketsMessages.ActionNames.AutoMoveTickets &&
            n.MessageType == MoveTicketsMessages.MessageTypes.Reminder &&
            n.MessageGroupId == MessageGroup.MatchReadyToMove &&
            n.LetstalkGroup == LetstalkGroup.Notification
        ));
    }

    [Fact]
    public async Task Execute_WhenExceptionThrown_LogsErrorAndRethrows()
    {
        // Arrange
        autoMoveStatusService.GetAutoMoveStatus().Returns(Task.FromException<bool>(new Exception("fail")));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => job.Execute(jobContext));
        logger.Received(1).LogError(Arg.Any<Exception>(), "Error occurred in MatchReadyToMoveJob");
        Assert.Equal("fail", ex.Message);
    }
}
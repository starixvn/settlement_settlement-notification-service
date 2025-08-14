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

public class MoveTicketsJobTests
{
    private readonly IAutoMoveStatusService autoMoveStatusService;
    private readonly IMatchesReadyToMoveService matchesReadyToMoveService;
    private readonly IMoveTicketService moveTicketService;
    private readonly NotificationService notificationService;
    private readonly INotificationContentService notificationContentService;

    private readonly MoveTicketsJob job;
    private readonly IJobExecutionContext jobContext;

    public MoveTicketsJobTests()
    {
        autoMoveStatusService = Substitute.For<IAutoMoveStatusService>();
        matchesReadyToMoveService = Substitute.For<IMatchesReadyToMoveService>();
        moveTicketService = Substitute.For<IMoveTicketService>();
        notificationService = Substitute.For<NotificationService>();
        notificationContentService = Substitute.For<INotificationContentService>();

        job = new MoveTicketsJob(autoMoveStatusService, matchesReadyToMoveService, moveTicketService, notificationService, notificationContentService);
        jobContext = Substitute.For<IJobExecutionContext>();
    }

    [Fact]
    public async Task Execute_AutoMoveDisabled_SendsNotificationOffSetting()
    {
        // Arrange
        autoMoveStatusService.GetAutoMoveStatus().Returns(false);

        // Act
        await job.Execute(jobContext);

        // Assert
        notificationService.Received(1).Notify(Arg.Is<NotificationModel>(n =>
            n.ActionName == MoveTicketsMessages.ActionNames.AutoMoveTickets &&
            n.MessageContent == MoveTicketsMessages.ContentTemplates.SettingStatusOffTemplate &&
            n.MessageType == MoveTicketsMessages.MessageTypes.Reminder &&
            n.MessageGroupId == MessageGroup.MatchReadyToMove
        ));
        await matchesReadyToMoveService.DidNotReceive().GetEligibleMatches();
        await moveTicketService.DidNotReceiveWithAnyArgs().ProcessMoveTicket(default);
    }

    [Fact]
    public async Task Execute_AutoMoveEnabled_ProcessesAllEligibleMatches()
    {
        // Arrange
        autoMoveStatusService.GetAutoMoveStatus().Returns(true);
        var matches = new List<Match> { new Match { MatchId = 1 }, new Match { MatchId = 2 } };
        matchesReadyToMoveService.GetEligibleMatches().Returns(matches);
        moveTicketService.ProcessMoveTicket(Arg.Any<Match>()).Returns(true);

        // Act
        await job.Execute(jobContext);

        // Assert
        await moveTicketService.Received(1).ProcessMoveTicket(matches[0]);
        await moveTicketService.Received(1).ProcessMoveTicket(matches[1]);
        notificationService.DidNotReceive().Notify(Arg.Is<NotificationModel>(n =>
            n.MessageContent == MoveTicketsMessages.ContentTemplates.NoMatchMovie
        ));
    }

    [Fact]
    public async Task Execute_AutoMoveEnabled_NoEligibleMatches_SendsNoMatchMoveNotification()
    {
        // Arrange
        autoMoveStatusService.GetAutoMoveStatus().Returns(true);
        matchesReadyToMoveService.GetEligibleMatches().Returns(new List<Match>());

        // Act
        await job.Execute(jobContext);

        // Assert
        await moveTicketService.DidNotReceiveWithAnyArgs().ProcessMoveTicket(default);
        notificationService.Received(1).Notify(Arg.Is<NotificationModel>(n =>
            n.ActionName == MoveTicketsMessages.ActionNames.AutoMoveTickets &&
            n.MessageContent == MoveTicketsMessages.ContentTemplates.NoMatchMovie &&
            n.MessageType == MoveTicketsMessages.MessageTypes.Reminder &&
            n.MessageGroupId == MessageGroup.MatchReadyToMove
        ));
    }

    [Fact]
    public async Task Execute_AutoMoveEnabled_AllMoveTicketFail_SendsNoMatchMoveNotification()
    {
        // Arrange
        autoMoveStatusService.GetAutoMoveStatus().Returns(true);
        var matches = new List<Match> { new Match { MatchId = 1 }, new Match { MatchId = 2 } };
        matchesReadyToMoveService.GetEligibleMatches().Returns(matches);
        moveTicketService.ProcessMoveTicket(Arg.Any<Match>()).Returns(false);

        // Act
        await job.Execute(jobContext);

        // Assert
        await moveTicketService.Received(1).ProcessMoveTicket(matches[0]);
        await moveTicketService.Received(1).ProcessMoveTicket(matches[1]);
        notificationService.Received(1).Notify(Arg.Is<NotificationModel>(n =>
            n.ActionName == MoveTicketsMessages.ActionNames.AutoMoveTickets &&
            n.MessageContent == MoveTicketsMessages.ContentTemplates.NoMatchMovie &&
            n.MessageType == MoveTicketsMessages.MessageTypes.Reminder &&
            n.MessageGroupId == MessageGroup.MatchReadyToMove
        ));
    }
}

using Notification.MoveTickets.Jobs;
using Notification.MoveTickets.Models;
using Notification.MoveTickets.Services;
using NSubstitute;
using Quartz;
using SettlementNotification.Services;
using Xunit;

namespace Notification.MoveTickets.UnitTests.Jobs;

public class MoveTicketsJobTests
{
    private readonly IAutoMoveStatusService autoMoveStatusService;
    private readonly IMatchesReadyToMoveService matchesReadyToMoveService;
    private readonly IMoveTicketService moveTicketService;
    private readonly NotificationService notificationService;

    private readonly MoveTicketsJob job;
    private readonly IJobExecutionContext jobContext;

    public MoveTicketsJobTests()
    {
        autoMoveStatusService = Substitute.For<IAutoMoveStatusService>();
        matchesReadyToMoveService = Substitute.For<IMatchesReadyToMoveService>();
        moveTicketService = Substitute.For<IMoveTicketService>();
        notificationService = Substitute.For<NotificationService>();

        job = new MoveTicketsJob(autoMoveStatusService, matchesReadyToMoveService, moveTicketService, notificationService);
        jobContext = Substitute.For<IJobExecutionContext>();
    }

    [Fact]
    public async Task Execute_AutoMoveDisabled_DoesNotProcessMatches()
    {
        // Arrange
        autoMoveStatusService.GetAutoMoveStatus().Returns(false);

        // Act
        await job.Execute(jobContext);

        // Assert
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

        // Act
        await job.Execute(jobContext);

        // Assert
        await moveTicketService.Received(1).ProcessMoveTicket(matches[0]);
        await moveTicketService.Received(1).ProcessMoveTicket(matches[1]);
    }

    [Fact]
    public async Task Execute_AutoMoveEnabled_NoEligibleMatches_DoesNotCallProcessMoveTicket()
    {
        // Arrange
        autoMoveStatusService.GetAutoMoveStatus().Returns(true);
        matchesReadyToMoveService.GetEligibleMatches().Returns(new List<Match>());

        // Act
        await job.Execute(jobContext);

        // Assert
        await moveTicketService.DidNotReceiveWithAnyArgs().ProcessMoveTicket(default);
    }
}

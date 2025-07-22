using Fanex.Caching;
using Microsoft.Extensions.Logging;
using Notification.MoveTickets.Features.GetMatchesReadyToMove;
using Notification.MoveTickets.Features.GetMatchTicketInfo;
using Notification.MoveTickets.Jobs;
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
    private readonly IGetMatchesReadyToMoveHandler getMatchesHandler;
    private readonly IGetMatchTicketInfoHandler getMatchTicketInfoHandler;
    private readonly ICacheService cacheService;
    private readonly IJobExecutionContext jobContext;
    private readonly MatchReadyToMoveJob job;

    public MatchReadyToMoveJobTests()
    {
        logger = Substitute.For<ILogger<MatchReadyToMoveJob>>();
        notificationService = Substitute.For<NotificationService>();
        getMatchesHandler = Substitute.For<IGetMatchesReadyToMoveHandler>();
        getMatchTicketInfoHandler = Substitute.For<IGetMatchTicketInfoHandler>();
        cacheService = Substitute.For<ICacheService>();
        jobContext = Substitute.For<IJobExecutionContext>();

        job = new MatchReadyToMoveJob(
            logger,
            notificationService,
            getMatchesHandler,
            getMatchTicketInfoHandler,
            cacheService);
    }


    [Fact]
    public async Task Execute_WithNoEligibleMatches_ShouldLogAndReturn()
    {
        // Arrange
        var matches = CreateTestMatches();
        var tickets = CreateTestTicketsWithNoSettledTickets();

        cacheService.ContainAsync("Settlement-AutoMoveStatus").Returns(true);
        cacheService.TryGetAsync<bool>("Settlement-AutoMoveStatus").Returns((true, true));
        getMatchesHandler.HandleAsync().Returns(matches);
        getMatchTicketInfoHandler.HandleAsync(Arg.Any<List<int>>()).Returns(tickets);

        // Act
        await job.Execute(jobContext);

        // Assert
        logger.Received(1).LogInformation("No eligible matches found for moving tickets");
        notificationService.DidNotReceive().Notify(Arg.Any<NotificationModel>());
    }

    [Fact]
    public async Task Execute_WithCacheFailure_ShouldDefaultToFalse()
    {
        // Arrange
        var matches = CreateTestMatches();
        var tickets = CreateTestTickets();

        cacheService.ContainAsync("Settlement-AutoMoveStatus").Returns(false);
        getMatchesHandler.HandleAsync().Returns(matches);
        getMatchTicketInfoHandler.HandleAsync(Arg.Any<List<int>>()).Returns(tickets);

        // Act
        await job.Execute(jobContext);

        // Assert
        notificationService.Received(1).Notify(Arg.Is<NotificationModel>(n =>
            n.MessageContent.Contains("Auto move tickets of settled results service = OFF")));
    }

    [Fact]
    public async Task Execute_WithException_ShouldLogErrorAndRethrow()
    {
        // Arrange
        getMatchesHandler.HandleAsync().Returns(Task.FromException<List<Match>>(new Exception("Test exception")));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => job.Execute(jobContext));

        logger.Received(1).LogError(Arg.Any<Exception>(), "Error occurred in MatchReadyToMoveJob");
        Assert.Equal("Test exception", exception.Message);
    }

    [Fact]
    public async Task Execute_WithBaseballAndCricketMatches_ShouldIncludeBothInContent()
    {
        // Arrange
        var matches = new List<Match>
        {
            new() { MatchId = 1, SportType = SportType.Baseball, SettlementStatus = 1 },
            new() { MatchId = 2, SportType = SportType.Cricket, SettlementStatus = 1 },
            new() { MatchId = 3, SportType = SportType.Soccer, SettlementStatus = 1 }
        };
        var tickets = new List<TicketInfo>
        {
            new() { MatchId = 1, IsExistsSettledTicket = true },
            new() { MatchId = 2, IsExistsSettledTicket = true },
            new() { MatchId = 3, IsExistsSettledTicket = true }
        };

        cacheService.ContainAsync("Settlement-AutoMoveStatus").Returns(true);
        cacheService.TryGetAsync<bool>("Settlement-AutoMoveStatus").Returns((true, true));
        getMatchesHandler.HandleAsync().Returns(matches);
        getMatchTicketInfoHandler.HandleAsync(Arg.Any<List<int>>()).Returns(tickets);

        // Act
        await job.Execute(jobContext);

        // Assert
        notificationService.Received(1).Notify(Arg.Is<NotificationModel>(n =>
            n.MessageContent.Contains("+ Baseball: 1") &&
            n.MessageContent.Contains("+ Cricket: 2")));
    }

    [Fact]
    public async Task Execute_WithOnHoldMatches_ShouldIncludeInContent()
    {
        // Arrange
        var matches = new List<Match>
        {
            new() { MatchId = 1, SportType = SportType.Baseball, SettlementStatus = 1 }, // Pending
            new() { MatchId = 2, SportType = SportType.Baseball, SettlementStatus = 2 }, // Undecided
            new() { MatchId = 3, SportType = SportType.Baseball, SettlementStatus = 4 }, // UnderSettle
            new() { MatchId = 4, SportType = SportType.Baseball, SettlementStatus = 16 }, // MtdHold
            new() { MatchId = 5, SportType = SportType.Baseball, SettlementStatus = 64 }  // KeyHold
        };
        var tickets = matches.Select(m => new TicketInfo { MatchId = m.MatchId, IsExistsSettledTicket = true }).ToList();

        cacheService.ContainAsync("Settlement-AutoMoveStatus").Returns(true);
        cacheService.TryGetAsync<bool>("Settlement-AutoMoveStatus").Returns((true, true));
        getMatchesHandler.HandleAsync().Returns(matches);
        getMatchTicketInfoHandler.HandleAsync(Arg.Any<List<int>>()).Returns(tickets);

        // Act
        await job.Execute(jobContext);

        // Assert
        notificationService.Received(1).Notify(Arg.Is<NotificationModel>(n =>
            n.MessageContent.Contains("+ matches on-hold: 1, 2, 3, 4, 5")));
    }

    [Fact]
    public async Task Execute_WithMatchesWithoutChild_ShouldIncludeInContent()
    {
        // Arrange
        var matches = new List<Match>
        {
            new() { MatchId = 1, SportType = SportType.Baseball, ChildMatchId = null },
            new() { MatchId = 2, SportType = SportType.Baseball, ChildMatchId = 0 },
            new() { MatchId = 3, SportType = SportType.Baseball, ChildMatchId = 100 }
        };
        var tickets = matches.Select(m => new TicketInfo { MatchId = m.MatchId, IsExistsSettledTicket = true }).ToList();

        cacheService.ContainAsync("Settlement-AutoMoveStatus").Returns(true);
        cacheService.TryGetAsync<bool>("Settlement-AutoMoveStatus").Returns((true, true));
        getMatchesHandler.HandleAsync().Returns(matches);
        getMatchTicketInfoHandler.HandleAsync(Arg.Any<List<int>>()).Returns(tickets);

        // Act
        await job.Execute(jobContext);

        // Assert
        notificationService.Received(1).Notify(Arg.Is<NotificationModel>(n =>
            n.MessageContent.Contains("+ matches without corresponding child-match to move tickets: 1, 2")));
    }

    [Fact]
    public async Task Execute_WithAutoMoveEnabled_ShouldShowONInContent()
    {
        // Arrange
        var matches = CreateTestMatches();
        var tickets = CreateTestTickets();

        cacheService.ContainAsync("Settlement-AutoMoveStatus").Returns(true);
        cacheService.TryGetAsync<bool>("Settlement-AutoMoveStatus").Returns((true, true));
        getMatchesHandler.HandleAsync().Returns(matches);
        getMatchTicketInfoHandler.HandleAsync(Arg.Any<List<int>>()).Returns(tickets);

        // Act
        await job.Execute(jobContext);

        // Assert
        notificationService.Received(1).Notify(Arg.Is<NotificationModel>(n =>
            n.MessageContent.Contains("Auto move tickets of settled results service = ON")));
    }

    [Fact]
    public async Task Execute_WithAutoMoveDisabled_ShouldShowOFFInContent()
    {
        // Arrange
        var matches = CreateTestMatches();
        var tickets = CreateTestTickets();

        cacheService.ContainAsync("Settlement-AutoMoveStatus").Returns(true);
        cacheService.TryGetAsync<bool>("Settlement-AutoMoveStatus").Returns((true, false));
        getMatchesHandler.HandleAsync().Returns(matches);
        getMatchTicketInfoHandler.HandleAsync(Arg.Any<List<int>>()).Returns(tickets);

        // Act
        await job.Execute(jobContext);

        // Assert
        notificationService.Received(1).Notify(Arg.Is<NotificationModel>(n =>
            n.MessageContent.Contains("Auto move tickets of settled results service = OFF")));
    }

    [Fact]
    public async Task Execute_WithEmptyMatches_ShouldReturnEarly()
    {
        // Arrange
        cacheService.ContainAsync("Settlement-AutoMoveStatus").Returns(true);
        cacheService.TryGetAsync<bool>("Settlement-AutoMoveStatus").Returns((true, true));
        getMatchesHandler.HandleAsync().Returns(new List<Match>());
        getMatchTicketInfoHandler.HandleAsync(Arg.Any<List<int>>()).Returns(new List<TicketInfo>());

        // Act
        await job.Execute(jobContext);

        // Assert
        logger.Received(1).LogInformation("No eligible matches found for moving tickets");
        notificationService.DidNotReceive().Notify(Arg.Any<NotificationModel>());
    }

    [Fact]
    public async Task Execute_WithNotificationModel_ShouldHaveCorrectProperties()
    {
        // Arrange
        var matches = CreateTestMatches();
        var tickets = CreateTestTickets();

        cacheService.ContainAsync("Settlement-AutoMoveStatus").Returns(true);
        cacheService.TryGetAsync<bool>("Settlement-AutoMoveStatus").Returns((true, true));
        getMatchesHandler.HandleAsync().Returns(matches);
        getMatchTicketInfoHandler.HandleAsync(Arg.Any<List<int>>()).Returns(tickets);

        // Act
        await job.Execute(jobContext);

        // Assert
        notificationService.Received(1).Notify(Arg.Is<NotificationModel>(n =>
            n.ActionName == "Auto move tickets of settled results" &&
            n.MessageGroupId == MessageGroup.MatchReadyToMove &&
            n.LetstalkGroup == LetstalkGroup.Notification &&
            !string.IsNullOrEmpty(n.MessageContent)));
    }

    private static List<Match> CreateTestMatches()
    {
        return new List<Match>
        {
            new() { MatchId = 1, SportType = SportType.Baseball, SettlementStatus = 0 },
            new() { MatchId = 2, SportType = SportType.Cricket, SettlementStatus = 0 },
            new() { MatchId = 3, SportType = SportType.Baseball, SettlementStatus = 0 }
        };
    }

    private static List<TicketInfo> CreateTestTickets()
    {
        return new List<TicketInfo>
        {
            new() { MatchId = 1, IsExistsSettledTicket = true },
            new() { MatchId = 2, IsExistsSettledTicket = true },
            new() { MatchId = 3, IsExistsSettledTicket = true }
        };
    }

    private static List<TicketInfo> CreateTestTicketsWithNoSettledTickets()
    {
        return new List<TicketInfo>
        {
            new() { MatchId = 1, IsExistsSettledTicket = false },
            new() { MatchId = 2, IsExistsSettledTicket = false },
            new() { MatchId = 3, IsExistsSettledTicket = false }
        };
    }
}

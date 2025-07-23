using Fanex.Caching;
using Microsoft.Extensions.Logging;
using Notification.MoveTickets.Features.GetMatchesReadyToMove;
using Notification.MoveTickets.Features.GetMatchTicketInfo;
using Notification.MoveTickets.Jobs;
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
    private readonly IGetMatchesReadyToMoveHandler getMatchesHandler;
    private readonly IGetMatchTicketInfoHandler getMatchTicketInfoHandler;
    private readonly ICacheService cacheService;
    private readonly INotificationContentService notificationContentService;
    private readonly IJobExecutionContext jobContext;
    private readonly MatchReadyToMoveJob job;

    public MatchReadyToMoveJobTests()
    {
        logger = Substitute.For<ILogger<MatchReadyToMoveJob>>();
        notificationService = Substitute.For<NotificationService>();
        getMatchesHandler = Substitute.For<IGetMatchesReadyToMoveHandler>();
        getMatchTicketInfoHandler = Substitute.For<IGetMatchTicketInfoHandler>();
        cacheService = Substitute.For<ICacheService>();
        notificationContentService = Substitute.For<INotificationContentService>();
        jobContext = Substitute.For<IJobExecutionContext>();

        job = new MatchReadyToMoveJob(
            logger,
            notificationService,
            getMatchesHandler,
            getMatchTicketInfoHandler,
            cacheService,
            notificationContentService);
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
        notificationContentService.CreateNotificationContent(Arg.Any<List<Match>>(), Arg.Any<bool>()).Returns("Test content");

        // Act
        await job.Execute(jobContext);

        // Assert
        notificationService.Received(1).Notify(Arg.Is<NotificationModel>(n =>
            n.MessageContent == "Test content"));
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
    public async Task Execute_WithEligibleMatches_ShouldCallNotificationContentService()
    {
        // Arrange
        var matches = CreateTestMatches();
        var tickets = CreateTestTickets();

        cacheService.ContainAsync("Settlement-AutoMoveStatus").Returns(true);
        cacheService.TryGetAsync<bool>("Settlement-AutoMoveStatus").Returns((true, true));
        getMatchesHandler.HandleAsync().Returns(matches);
        getMatchTicketInfoHandler.HandleAsync(Arg.Any<List<int>>()).Returns(tickets);
        notificationContentService.CreateNotificationContent(Arg.Any<List<Match>>(), Arg.Any<bool>()).Returns("Test content");

        // Act
        await job.Execute(jobContext);

        // Assert
        notificationContentService.Received(1).CreateNotificationContent(Arg.Is<List<Match>>(m => m.Count == 3), true);
        notificationService.Received(1).Notify(Arg.Is<NotificationModel>(n => n.MessageContent == "Test content"));
    }

    [Fact]
    public async Task Execute_WithAutoMoveEnabled_ShouldPassTrueToContentService()
    {
        // Arrange
        var matches = CreateTestMatches();
        var tickets = CreateTestTickets();

        cacheService.ContainAsync("Settlement-AutoMoveStatus").Returns(true);
        cacheService.TryGetAsync<bool>("Settlement-AutoMoveStatus").Returns((true, true));
        getMatchesHandler.HandleAsync().Returns(matches);
        getMatchTicketInfoHandler.HandleAsync(Arg.Any<List<int>>()).Returns(tickets);
        notificationContentService.CreateNotificationContent(Arg.Any<List<Match>>(), Arg.Any<bool>()).Returns("Test content");

        // Act
        await job.Execute(jobContext);

        // Assert
        notificationContentService.Received(1).CreateNotificationContent(Arg.Any<List<Match>>(), true);
    }

    [Fact]
    public async Task Execute_WithAutoMoveDisabled_ShouldPassFalseToContentService()
    {
        // Arrange
        var matches = CreateTestMatches();
        var tickets = CreateTestTickets();

        cacheService.ContainAsync("Settlement-AutoMoveStatus").Returns(true);
        cacheService.TryGetAsync<bool>("Settlement-AutoMoveStatus").Returns((true, false));
        getMatchesHandler.HandleAsync().Returns(matches);
        getMatchTicketInfoHandler.HandleAsync(Arg.Any<List<int>>()).Returns(tickets);
        notificationContentService.CreateNotificationContent(Arg.Any<List<Match>>(), Arg.Any<bool>()).Returns("Test content");

        // Act
        await job.Execute(jobContext);

        // Assert
        notificationContentService.Received(1).CreateNotificationContent(Arg.Any<List<Match>>(), false);
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
        notificationService.DidNotReceive().Notify(Arg.Any<NotificationModel>());
        notificationContentService.DidNotReceive().CreateNotificationContent(Arg.Any<List<Match>>(), Arg.Any<bool>());
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
        notificationContentService.CreateNotificationContent(Arg.Any<List<Match>>(), Arg.Any<bool>()).Returns("Test content");

        // Act
        await job.Execute(jobContext);

        // Assert
        notificationService.Received(1).Notify(Arg.Is<NotificationModel>(n =>
            n.ActionName == "Auto move tickets of settled results" &&
            n.MessageGroupId == MessageGroup.MatchReadyToMove &&
            n.LetstalkGroup == LetstalkGroup.Notification &&
            n.MessageContent == "Test content"));
    }

    [Fact]
    public async Task Execute_WithCacheException_ShouldLogWarningAndReturnFalse()
    {
        // Arrange
        var matches = CreateTestMatches();
        var tickets = CreateTestTickets();

        cacheService.ContainAsync("Settlement-AutoMoveStatus").Returns(Task.FromException<bool>(new Exception("Cache error")));
        getMatchesHandler.HandleAsync().Returns(matches);
        getMatchTicketInfoHandler.HandleAsync(Arg.Any<List<int>>()).Returns(tickets);
        notificationContentService.CreateNotificationContent(Arg.Any<List<Match>>(), Arg.Any<bool>()).Returns("Test content");

        // Act
        await job.Execute(jobContext);

        // Assert
        notificationContentService.Received(1).CreateNotificationContent(Arg.Any<List<Match>>(), false);
    }

    [Fact]
    public async Task Execute_WithCacheKeyNotExists_ShouldReturnFalse()
    {
        // Arrange
        var matches = CreateTestMatches();
        var tickets = CreateTestTickets();

        cacheService.ContainAsync("Settlement-AutoMoveStatus").Returns(false);
        getMatchesHandler.HandleAsync().Returns(matches);
        getMatchTicketInfoHandler.HandleAsync(Arg.Any<List<int>>()).Returns(tickets);
        notificationContentService.CreateNotificationContent(Arg.Any<List<Match>>(), Arg.Any<bool>()).Returns("Test content");

        // Act
        await job.Execute(jobContext);

        // Assert
        notificationContentService.Received(1).CreateNotificationContent(Arg.Any<List<Match>>(), false);
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
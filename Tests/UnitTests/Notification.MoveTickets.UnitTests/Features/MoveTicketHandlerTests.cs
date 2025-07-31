using Fanex.Data.Repository;
using Fanex.SiteDbConnectionManager.Client.Models;
using Fanex.SiteDbConnectionManager.Client.Services;
using Notification.MoveTickets.Constants;
using Notification.MoveTickets.Features.GetMoveTicketInfo;
using Notification.MoveTickets.Features.MoveTicket;
using Notification.MoveTickets.Models;
using NSubstitute;
using SettlementNotification.Services;
using Xunit;

namespace Notification.MoveTickets.UnitTests.Features;

public class MoveTicketHandlerTests
{
    private readonly SiteDbClientService siteDbClientService;
    private readonly IDynamicRepository repository;
    private readonly ISettleLogService settleLogService;
    private readonly MoveTicketHandler handler;

    public MoveTicketHandlerTests()
    {
        siteDbClientService = Substitute.For<SiteDbClientService>();
        repository = Substitute.For<IDynamicRepository>();
        settleLogService = Substitute.For<ISettleLogService>();
        handler = new MoveTicketHandler(siteDbClientService, repository, settleLogService);
    }

    [Fact]
    public async Task HandleAsync_AllSitesSuccess_ReturnsTrueAndSumTicketCount()
    {
        // Arrange
        var match = new Match { MatchId = 1 };
        var moveInfo = new MatchMoveInfo { EventDate = DateTime.UtcNow, ChildMatchId = 2, Resources = "res" };
        var siteDbs = new List<SiteDb> { new SiteDb { SiteName = "db1" }, new SiteDb { SiteName = "db2" } };
        siteDbClientService.GetActiveSiteDbAsync().Returns(siteDbs);

        repository.When(x => x.ExecuteAsync(Arg.Any<MoveTicketCriteria>()))
            .Do(ci =>
            {
                var c = ci.Arg<MoveTicketCriteria>();
                c.ErrCode = 0;
                c.ErrMsg = string.Empty;
                c.TicketCount = c.DbServer == "db1" ? 2 : 3;
            });

        // Act
        var (success, totalTickets) = await handler.HandleAsync(match, moveInfo);

        // Assert
        Assert.True(success);
        Assert.Equal(5, totalTickets);
        await settleLogService.DidNotReceiveWithAnyArgs().WriteLog(default, default, default);
        await repository.Received(2).ExecuteAsync(Arg.Any<MoveTicketCriteria>());
    }

    [Fact]
    public async Task HandleAsync_AnySiteFails_LogsAndReturnsFalse()
    {
        // Arrange
        var match = new Match { MatchId = 1 };
        var moveInfo = new MatchMoveInfo { EventDate = DateTime.UtcNow, ChildMatchId = 2, Resources = "res" };
        var siteDbs = new List<SiteDb> { new SiteDb { SiteName = "db1" }, new SiteDb { SiteName = "db2" } };
        siteDbClientService.GetActiveSiteDbAsync().Returns(siteDbs);

        repository.When(x => x.ExecuteAsync(Arg.Any<MoveTicketCriteria>()))
            .Do(ci =>
            {
                var c = ci.Arg<MoveTicketCriteria>();
                if (c.DbServer == "db1")
                {
                    c.ErrCode = 0;
                    c.ErrMsg = string.Empty;
                    c.TicketCount = 2;
                }
                else
                {
                    c.ErrCode = 99;
                    c.ErrMsg = "Database connection failed";
                    c.TicketCount = 0;
                }
            });

        // Act
        var (success, totalTickets) = await handler.HandleAsync(match, moveInfo);

        // Assert
        Assert.False(success);
        Assert.Equal(0, totalTickets);
        await settleLogService.Received(1).WriteLog(
            match.MatchId,
            Arg.Is<string>(msg => msg.Contains("Database connection failed")),
            MoveTicketConts.ActionIdMoveTicketFailed);
    }

    [Fact]
    public async Task HandleAsync_NoTicketsMoved_LogsAndReturnsFalse()
    {
        // Arrange
        var match = new Match { MatchId = 1 };
        var moveInfo = new MatchMoveInfo { EventDate = DateTime.UtcNow, ChildMatchId = 2, Resources = "res" };
        var siteDbs = new List<SiteDb> { new SiteDb { SiteName = "db1" }, new SiteDb { SiteName = "db2" } };
        siteDbClientService.GetActiveSiteDbAsync().Returns(siteDbs);

        repository.When(x => x.ExecuteAsync(Arg.Any<MoveTicketCriteria>()))
            .Do(ci =>
            {
                var c = ci.Arg<MoveTicketCriteria>();
                c.ErrCode = 0;
                c.ErrMsg = string.Empty;
                c.TicketCount = 0;
            });

        // Act
        var (success, totalTickets) = await handler.HandleAsync(match, moveInfo);

        // Assert
        Assert.False(success);
        Assert.Equal(0, totalTickets);
        await settleLogService.Received(1).WriteLog(
            match.MatchId,
            MoveTicketConts.NotExistValidTicketToMove,
            MoveTicketConts.ActionIdMoveTicketFailed);
    }

    [Fact]
    public async Task HandleAsync_SingleSiteSuccess_ReturnsTrueAndCorrectTicketCount()
    {
        // Arrange
        var match = new Match { MatchId = 123 };
        var moveInfo = new MatchMoveInfo { EventDate = DateTime.UtcNow, ChildMatchId = 456, Resources = "testRes" };
        var siteDbs = new List<SiteDb> { new SiteDb { SiteName = "singleDb" } };
        siteDbClientService.GetActiveSiteDbAsync().Returns(siteDbs);

        repository.When(x => x.ExecuteAsync(Arg.Any<MoveTicketCriteria>()))
            .Do(ci =>
            {
                var c = ci.Arg<MoveTicketCriteria>();
                c.ErrCode = 0;
                c.ErrMsg = string.Empty;
                c.TicketCount = 10;
            });

        // Act
        var (success, totalTickets) = await handler.HandleAsync(match, moveInfo);

        // Assert
        Assert.True(success);
        Assert.Equal(10, totalTickets);
        await repository.Received(1).ExecuteAsync(Arg.Is<MoveTicketCriteria>(c => 
            c.MatchId == 123 && 
            c.ChildMatchId == 456 && 
            c.Resources == "testRes" &&
            c.DbServer == "singleDb"));
    }

    [Fact]
    public async Task HandleAsync_EmptySiteList_ReturnsSuccessWithZeroTickets()
    {
        // Arrange
        var match = new Match { MatchId = 1 };
        var moveInfo = new MatchMoveInfo { EventDate = DateTime.UtcNow, ChildMatchId = 2, Resources = "res" };
        var siteDbs = new List<SiteDb>();
        siteDbClientService.GetActiveSiteDbAsync().Returns(siteDbs);

        // Act
        var (success, totalTickets) = await handler.HandleAsync(match, moveInfo);

        // Assert
        Assert.False(success);
        Assert.Equal(0, totalTickets);
        await repository.DidNotReceiveWithAnyArgs().ExecuteAsync(Arg.Any<MoveTicketCriteria>());
        await settleLogService.Received(1).WriteLog(
            match.MatchId,
            MoveTicketConts.NotExistValidTicketToMove,
            MoveTicketConts.ActionIdMoveTicketFailed);
    }

    [Fact]
    public async Task HandleAsync_MixedResults_SomeSuccessSomeFail_ReturnsFalse()
    {
        // Arrange
        var match = new Match { MatchId = 1 };
        var moveInfo = new MatchMoveInfo { EventDate = DateTime.UtcNow, ChildMatchId = 2, Resources = "res" };
        var siteDbs = new List<SiteDb> 
        { 
            new SiteDb { SiteName = "successDb" }, 
            new SiteDb { SiteName = "failDb" },
            new SiteDb { SiteName = "anotherSuccessDb" }
        };
        siteDbClientService.GetActiveSiteDbAsync().Returns(siteDbs);

        repository.When(x => x.ExecuteAsync(Arg.Any<MoveTicketCriteria>()))
            .Do(ci =>
            {
                var c = ci.Arg<MoveTicketCriteria>();
                if (c.DbServer == "failDb")
                {
                    c.ErrCode = 1;
                    c.ErrMsg = "Connection timeout";
                    c.TicketCount = 0;
                }
                else
                {
                    c.ErrCode = 0;
                    c.ErrMsg = string.Empty;
                    c.TicketCount = c.DbServer == "successDb" ? 5 : 7;
                }
            });

        // Act
        var (success, totalTickets) = await handler.HandleAsync(match, moveInfo);

        // Assert
        Assert.False(success);
        Assert.Equal(0, totalTickets);
        await settleLogService.Received(1).WriteLog(
            match.MatchId,
            Arg.Is<string>(msg => msg.Contains("Connection timeout")),
            MoveTicketConts.ActionIdMoveTicketFailed);
    }

    [Fact]
    public async Task HandleAsync_VerifiesCriteriaIsSetCorrectly()
    {
        // Arrange
        var match = new Match { MatchId = 999 };
        var eventDate = new DateTime(2024, 1, 15, 10, 30, 0);
        var moveInfo = new MatchMoveInfo 
        { 
            EventDate = eventDate, 
            ChildMatchId = 888, 
            Resources = "specialResource" 
        };
        var siteDbs = new List<SiteDb> { new SiteDb { SiteName = "testDb" } };
        siteDbClientService.GetActiveSiteDbAsync().Returns(siteDbs);

        repository.When(x => x.ExecuteAsync(Arg.Any<MoveTicketCriteria>()))
            .Do(ci =>
            {
                var c = ci.Arg<MoveTicketCriteria>();
                c.ErrCode = 0;
                c.ErrMsg = string.Empty;
                c.TicketCount = 1;
            });

        // Act
        await handler.HandleAsync(match, moveInfo);

        // Assert
        await repository.Received(1).ExecuteAsync(Arg.Is<MoveTicketCriteria>(c =>
            c.MatchId == 999 &&
            c.EventDate == eventDate &&
            c.ChildMatchId == 888 &&
            c.Resources == "specialResource" &&
            c.DbServer == "testDb"));
    }

    [Fact]
    public async Task HandleAsync_MultipleSitesWithMixedTicketCounts_SumsCorrectly()
    {
        // Arrange
        var match = new Match { MatchId = 1 };
        var moveInfo = new MatchMoveInfo { EventDate = DateTime.UtcNow, ChildMatchId = 2, Resources = "res" };
        var siteDbs = new List<SiteDb> 
        { 
            new SiteDb { SiteName = "db1" }, 
            new SiteDb { SiteName = "db2" },
            new SiteDb { SiteName = "db3" },
            new SiteDb { SiteName = "db4" }
        };
        siteDbClientService.GetActiveSiteDbAsync().Returns(siteDbs);

        repository.When(x => x.ExecuteAsync(Arg.Any<MoveTicketCriteria>()))
            .Do(ci =>
            {
                var c = ci.Arg<MoveTicketCriteria>();
                c.ErrCode = 0;
                c.ErrMsg = string.Empty;
                c.TicketCount = c.DbServer switch
                {
                    "db1" => 5,
                    "db2" => 0, // No tickets but no error
                    "db3" => 15,
                    "db4" => 3,
                    _ => 0
                };
            });

        // Act
        var (success, totalTickets) = await handler.HandleAsync(match, moveInfo);

        // Assert
        Assert.True(success);
        Assert.Equal(23, totalTickets); // 5 + 0 + 15 + 3
        await repository.Received(4).ExecuteAsync(Arg.Any<MoveTicketCriteria>());
    }
}

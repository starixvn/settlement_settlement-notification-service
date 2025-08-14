using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
using Notification.MoveTickets.Features.GetMatchTicketInfo;
using Notification.MoveTickets.Models;
using NSubstitute;
using SettlementNotification.Models;
using Xunit;

namespace Notification.MoveTickets.UnitTests.Features;

public class GetMatchTicketInfoHandlerTests
{
    private readonly IDynamicRepository repositorySub;
    private readonly IOptions<AppSettingsKey> settingOptionsSub;
    private readonly GetMatchTicketInfoHandler handler;

    public GetMatchTicketInfoHandlerTests()
    {
        repositorySub = Substitute.For<IDynamicRepository>();
        settingOptionsSub = Substitute.For<IOptions<AppSettingsKey>>();

        var appSettings = new AppSettingsKey
        {
            SiteDbServer = new List<string> { "DBTXN", "DBSITE01", "DBSITE03" }
        };
        settingOptionsSub.Value.Returns(appSettings);

        handler = new GetMatchTicketInfoHandler(repositorySub, settingOptionsSub);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyMatches_ReturnsEmptyList()
    {
        // Arrange
        var matches = new List<Match>();

        // Act
        var result = await handler.HandleAsync(matches);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task HandleAsync_WithSingleMatch_ProcessesCorrectly()
    {
        // Arrange
        var matches = new List<Match>
        {
            new Match { MatchId = 1, SportType = 1, ResourceIds = new List<string> { "RES1", "RES2" } }
        };

        var expectedTicketInfos = new List<TicketInfo>
        {
            new TicketInfo { MatchId = 1, IsExistsTicket = true },
            new TicketInfo { MatchId = 1, IsExistsTicket = false }
        };

        repositorySub.FetchAsync<TicketInfo>(Arg.Any<GetMatchTicketInfoCriteria>())
            .Returns(expectedTicketInfos);

        // Act
        var result = await handler.HandleAsync(matches);

        // Assert
        Assert.Single(result);
        var ticketInfo = result.First();
        Assert.Equal(1, ticketInfo.MatchId);
        Assert.True(ticketInfo.IsExistsTicket); // Should be true because one exists
    }

    [Fact]
    public async Task HandleAsync_WithMultipleMatches_ProcessesAllCorrectly()
    {
        // Arrange
        var matches = new List<Match>
        {
            new Match { MatchId = 1, SportType = 1, ResourceIds = new List<string> { "RES1" } },
            new Match { MatchId = 2, SportType = 8, ResourceIds = new List<string> { "RES2", "RES3" } }
        };

        var expectedTicketInfos = new List<TicketInfo>
        {
            new TicketInfo { MatchId = 1, IsExistsTicket = true },
            new TicketInfo { MatchId = 2, IsExistsTicket = false },
            new TicketInfo { MatchId = 2, IsExistsTicket = true }
        };

        repositorySub.FetchAsync<TicketInfo>(Arg.Any<GetMatchTicketInfoCriteria>())
            .Returns(expectedTicketInfos);

        // Act
        var result = await handler.HandleAsync(matches);

        // Assert
        Assert.Equal(2, result.Count);

        var match1Ticket = result.First(t => t.MatchId == 1);
        var match2Ticket = result.First(t => t.MatchId == 2);

        Assert.True(match1Ticket.IsExistsTicket);
        Assert.True(match2Ticket.IsExistsTicket); // Should be true because one exists
    }


    [Fact]
    public async Task HandleAsync_WithNoExistingTickets_ReturnsFalseForIsExistsTicket()
    {
        // Arrange
        var matches = new List<Match>
        {
            new Match { MatchId = 1, SportType = 1, ResourceIds = new List<string> { "RES1", "RES2" } }
        };

        var expectedTicketInfos = new List<TicketInfo>
        {
            new TicketInfo { MatchId = 1, IsExistsTicket = false },
            new TicketInfo { MatchId = 1, IsExistsTicket = false }
        };

        repositorySub.FetchAsync<TicketInfo>(Arg.Any<GetMatchTicketInfoCriteria>())
            .Returns(expectedTicketInfos);

        // Act
        var result = await handler.HandleAsync(matches);

        // Assert
        Assert.Single(result);
        var ticketInfo = result.First();
        Assert.Equal(1, ticketInfo.MatchId);
        Assert.False(ticketInfo.IsExistsTicket);
    }

    [Fact]
    public async Task HandleAsync_WithMixedTicketExistence_ReturnsTrueIfAnyExists()
    {
        // Arrange
        var matches = new List<Match>
        {
            new Match { MatchId = 1, SportType = 1, ResourceIds = new List<string> { "RES1", "RES2", "RES3" } }
        };

        var expectedTicketInfos = new List<TicketInfo>
        {
            new TicketInfo { MatchId = 1, IsExistsTicket = false },
            new TicketInfo { MatchId = 1, IsExistsTicket = true },
            new TicketInfo { MatchId = 1, IsExistsTicket = false }
        };

        repositorySub.FetchAsync<TicketInfo>(Arg.Any<GetMatchTicketInfoCriteria>())
            .Returns(expectedTicketInfos);

        // Act
        var result = await handler.HandleAsync(matches);

        // Assert
        Assert.Single(result);
        var ticketInfo = result.First();
        Assert.Equal(1, ticketInfo.MatchId);
        Assert.True(ticketInfo.IsExistsTicket); // Should be true because one exists
    }

    [Fact]
    public async Task HandleAsync_WithLargeResourceIds_HandlesCorrectly()
    {
        // Arrange
        var resourceIds = Enumerable.Range(1, 100).Select(i => $"RES{i}").ToList();
        var matches = new List<Match>
        {
            new Match { MatchId = 1, SportType = 1, ResourceIds = resourceIds }
        };

        var expectedTicketInfos = resourceIds.Select(_ => new TicketInfo { MatchId = 1, IsExistsTicket = false }).ToList();

        repositorySub.FetchAsync<TicketInfo>(Arg.Any<GetMatchTicketInfoCriteria>())
            .Returns(expectedTicketInfos);

        // Act
        var result = await handler.HandleAsync(matches);

        // Assert
        Assert.Single(result);
        var ticketInfo = result.First();
        Assert.Equal(1, ticketInfo.MatchId);
        Assert.False(ticketInfo.IsExistsTicket);
    }

    [Fact]
    public async Task HandleAsync_WithDifferentSportTypes_ProcessesCorrectly()
    {
        // Arrange
        var matches = new List<Match>
        {
            new Match { MatchId = 1, SportType = 1, ResourceIds = new List<string> { "RES1" } }, // Soccer
            new Match { MatchId = 2, SportType = 8, ResourceIds = new List<string> { "RES2" } }, // Baseball
            new Match { MatchId = 3, SportType = 50, ResourceIds = new List<string> { "RES3" } } // Cricket
        };

        var expectedTicketInfos = new List<TicketInfo>
        {
            new TicketInfo { MatchId = 1, IsExistsTicket = true },
            new TicketInfo { MatchId = 2, IsExistsTicket = false },
            new TicketInfo { MatchId = 3, IsExistsTicket = true }
        };

        repositorySub.FetchAsync<TicketInfo>(Arg.Any<GetMatchTicketInfoCriteria>())
            .Returns(expectedTicketInfos);

        // Act
        var result = await handler.HandleAsync(matches);

        // Assert
        Assert.Equal(3, result.Count);

        var soccerTicket = result.First(t => t.MatchId == 1);
        var baseballTicket = result.First(t => t.MatchId == 2);
        var cricketTicket = result.First(t => t.MatchId == 3);

        Assert.True(soccerTicket.IsExistsTicket);
        Assert.False(baseballTicket.IsExistsTicket);
        Assert.True(cricketTicket.IsExistsTicket);
    }




    [Fact]
    public async Task HandleAsync_WithMultipleSitesReturningEmpty_HandlesCorrectly()
    {
        // Arrange
        var matches = new List<Match>
        {
            new Match { MatchId = 1, SportType = 1, ResourceIds = new List<string> { "RES1" } }
        };

        // All sites return empty results
        repositorySub.FetchAsync<TicketInfo>(Arg.Any<GetMatchTicketInfoCriteria>())
            .Returns(new List<TicketInfo>());

        // Act
        var result = await handler.HandleAsync(matches);

        // Assert
        Assert.Empty(result);
    }



    [Fact]
    public async Task HandleAsync_WithAggregationLogic_CombinesResultsCorrectly()
    {
        // Arrange
        var matches = new List<Match>
        {
            new Match { MatchId = 1, SportType = 1, ResourceIds = new List<string> { "RES1", "RES2", "RES3" } }
        };

        var expectedTicketInfos = new List<TicketInfo>
        {
            new TicketInfo { MatchId = 1, IsExistsTicket = false },
            new TicketInfo { MatchId = 1, IsExistsTicket = false },
            new TicketInfo { MatchId = 1, IsExistsTicket = true }
        };

        repositorySub.FetchAsync<TicketInfo>(Arg.Any<GetMatchTicketInfoCriteria>())
            .Returns(expectedTicketInfos);

        // Act
        var result = await handler.HandleAsync(matches);

        // Assert
        Assert.Single(result);
        var ticketInfo = result.First();
        Assert.Equal(1, ticketInfo.MatchId);
        Assert.True(ticketInfo.IsExistsTicket); // Should be true because the last one exists

        // Verify that the aggregation logic works correctly
        // The handler should group by MatchId and set IsExistsTicket to true if any ticket exists
        Assert.True(result.All(t => t.IsExistsTicket == expectedTicketInfos.Any(et => et.MatchId == t.MatchId && et.IsExistsTicket)));
    }
}
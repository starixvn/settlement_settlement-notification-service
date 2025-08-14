using Fanex.Data.Repository;
using Notification.MoveTickets.Features.GetMatchesReadyToMove;
using Notification.MoveTickets.Models;
using NSubstitute;
using Xunit;

namespace Notification.MoveTickets.UnitTests.Features;

public class GetMatchesReadyToMoveHandlerTests
{
    private readonly IDynamicRepository repositorySub;
    private readonly GetMatchesReadyToMoveHandler handler;

    public GetMatchesReadyToMoveHandlerTests()
    {
        repositorySub = Substitute.For<IDynamicRepository>();
        handler = new GetMatchesReadyToMoveHandler(repositorySub);
    }

    [Fact]
    public async Task HandleAsync_ReturnsMatchesFromRepository()
    {
        // Arrange
        var expectedMatches = new List<Match>
        {
            new Match { MatchId = 1, SportType = 1, ResourceId = "RES1", SessionTime = 100, GameSession = 1, KickOffTime = DateTime.Now, ChildMatchId = 100, SettlementStatus = 1, IsTest = false },
            new Match { MatchId = 2, SportType = 2, ResourceId = "RES2", SessionTime = 200, GameSession = 2, KickOffTime = DateTime.Now.AddDays(1), ChildMatchId = 200, SettlementStatus = 2, IsTest = true }
        };
        repositorySub.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>())
            .Returns(expectedMatches);

        // Act
        var result = await handler.HandleAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].MatchId);
        Assert.Equal(2, result[1].MatchId);
        Assert.Equal(1, result[0].SportType);
        Assert.Equal(2, result[1].SportType);
    }

    [Fact]
    public async Task HandleAsync_RepositoryReturnsEmpty_ReturnsEmptyList()
    {
        // Arrange
        repositorySub.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>())
            .Returns(new List<Match>());

        // Act
        var result = await handler.HandleAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateMatchIds_GroupsAndDeduplicatesCorrectly()
    {
        // Arrange
        var inputMatches = new List<Match>
        {
            new Match { MatchId = 1, SportType = 1, ResourceId = "RES1", SessionTime = 100, GameSession = 1, KickOffTime = DateTime.Now, ChildMatchId = 100, SettlementStatus = 1, IsTest = false },
            new Match { MatchId = 1, SportType = 1, ResourceId = "RES2", SessionTime = 100, GameSession = 1, KickOffTime = DateTime.Now, ChildMatchId = 100, SettlementStatus = 1, IsTest = false },
            new Match { MatchId = 2, SportType = 2, ResourceId = "RES3", SessionTime = 200, GameSession = 2, KickOffTime = DateTime.Now.AddDays(1), ChildMatchId = 200, SettlementStatus = 2, IsTest = true }
        };
        repositorySub.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>())
            .Returns(inputMatches);

        // Act
        var result = await handler.HandleAsync();

        // Assert
        Assert.Equal(2, result.Count); // Should be grouped by MatchId

        var match1 = result.First(m => m.MatchId == 1);
        var match2 = result.First(m => m.MatchId == 2);

        // First match should have both ResourceIds
        Assert.Equal(2, match1.ResourceIds.Count);
        Assert.Contains("RES1", match1.ResourceIds);
        Assert.Contains("RES2", match1.ResourceIds);

        // Second match should have one ResourceId
        Assert.Equal(1, match2.ResourceIds.Count);
        Assert.Contains("RES3", match2.ResourceIds);
    }

    [Fact]
    public async Task HandleAsync_WithMultipleResourcesForSameMatch_CollectsAllResourceIds()
    {
        // Arrange
        var inputMatches = new List<Match>
        {
            new Match { MatchId = 1, ResourceId = "RES1" },
            new Match { MatchId = 1, ResourceId = "RES2" },
            new Match { MatchId = 1, ResourceId = "RES3" }
        };
        repositorySub.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>())
            .Returns(inputMatches);

        // Act
        var result = await handler.HandleAsync();

        // Assert
        Assert.Single(result);
        var match = result.First();
        Assert.Equal(1, match.MatchId);
        Assert.Equal(3, match.ResourceIds.Count);
        Assert.Contains("RES1", match.ResourceIds);
        Assert.Contains("RES2", match.ResourceIds);
        Assert.Contains("RES3", match.ResourceIds);
    }

    [Fact]
    public async Task HandleAsync_WithNullValues_HandlesGracefully()
    {
        // Arrange
        var inputMatches = new List<Match>
        {
            new Match { MatchId = 1, SportType = 1, ResourceId = "RES1", SessionTime = 100, GameSession = 1, KickOffTime = null, ChildMatchId = null, SettlementStatus = 0, IsTest = false }
        };
        repositorySub.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>())
            .Returns(inputMatches);

        // Act
        var result = await handler.HandleAsync();

        // Assert
        Assert.Single(result);
        var match = result.First();
        Assert.Equal(1, match.MatchId);
        Assert.Null(match.KickOffTime);
        Assert.Null(match.ChildMatchId);
    }

    [Fact]
    public async Task HandleAsync_WithZeroValues_HandlesCorrectly()
    {
        // Arrange
        var inputMatches = new List<Match>
        {
            new Match { MatchId = 1, SportType = 0, ResourceId = "RES1", SessionTime = 0, GameSession = 0, KickOffTime = DateTime.MinValue, ChildMatchId = 0, SettlementStatus = 0, IsTest = false }
        };
        repositorySub.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>())
            .Returns(inputMatches);

        // Act
        var result = await handler.HandleAsync();

        // Assert
        Assert.Single(result);
        var match = result.First();
        Assert.Equal(1, match.MatchId);
        Assert.Equal(0, match.SportType);
        Assert.Equal(0, match.SessionTime);
        Assert.Equal(0, match.GameSession);
        Assert.Equal(0, match.ChildMatchId);
        Assert.Equal(0, match.SettlementStatus);
    }

    [Fact]
    public async Task HandleAsync_WithLargeDataSet_ProcessesCorrectly()
    {
        // Arrange
        var inputMatches = new List<Match>();
        for (int i = 1; i <= 100; i++)
        {
            inputMatches.Add(new Match
            {
                MatchId = i,
                SportType = (short)(i % 3 + 1),
                ResourceId = $"RES{i}",
                SessionTime = i * 100,
                GameSession = i,
                KickOffTime = DateTime.Now.AddDays(i),
                ChildMatchId = i * 10,
                SettlementStatus = (byte)(i % 4),
                IsTest = i % 2 == 0
            });
        }
        repositorySub.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>())
            .Returns(inputMatches);

        // Act
        var result = await handler.HandleAsync();

        // Assert
        Assert.Equal(100, result.Count);
        Assert.Equal(100, result.Max(m => m.MatchId));
        Assert.Equal(1, result.Min(m => m.MatchId));
    }

    [Fact]
    public async Task HandleAsync_WithMixedDataTypes_HandlesCorrectly()
    {
        // Arrange
        var inputMatches = new List<Match>
        {
            new Match { MatchId = 1, SportType = 1, ResourceId = "RES1", SessionTime = 100, GameSession = 1, KickOffTime = DateTime.Now, ChildMatchId = 100, SettlementStatus = 1, IsTest = true },
            new Match { MatchId = 2, SportType = 8, ResourceId = "RES2", SessionTime = 200, GameSession = 2, KickOffTime = DateTime.Now.AddDays(1), ChildMatchId = 200, SettlementStatus = 2, IsTest = false },
            new Match { MatchId = 3, SportType = 50, ResourceId = "RES3", SessionTime = 300, GameSession = 3, KickOffTime = DateTime.Now.AddDays(2), ChildMatchId = 300, SettlementStatus = 3, IsTest = true }
        };
        repositorySub.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>())
            .Returns(inputMatches);

        // Act
        var result = await handler.HandleAsync();

        // Assert
        Assert.Equal(3, result.Count);

        var soccerMatch = result.First(m => m.SportType == 1);
        var baseballMatch = result.First(m => m.SportType == 8);
        var cricketMatch = result.First(m => m.SportType == 50);

        Assert.NotNull(soccerMatch);
        Assert.NotNull(baseballMatch);
        Assert.NotNull(cricketMatch);
    }


    [Fact]
    public async Task HandleAsync_WithGetMatchesReadyToMoveCriteria_CallsRepositoryCorrectly()
    {
        // Arrange
        var expectedMatches = new List<Match>
        {
            new Match { MatchId = 1, ResourceId = "RES1" }
        };
        repositorySub.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>())
            .Returns(expectedMatches);

        // Act
        await handler.HandleAsync();

        // Assert
        await repositorySub.Received(1).FetchAsync<Match>(Arg.Is<GetMatchesReadyToMoveCriteria>(c => c != null));
    }
}

using Fanex.Caching;
using Fanex.Data.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.MoveTickets.Features.GetMatchesReadyToMove;
using Notification.MoveTickets.Models;
using NSubstitute;
using SettlementNotification.Models;
using Xunit;

namespace Notification.MoveTickets.UnitTests.Features;

public class GetMatchesReadyToMoveHandlerTests
{
    private readonly IDynamicRepository repository;
    private readonly IOptions<AppSettingsKey> appSettingsKey;
    private readonly ICache cache;
    private readonly GetMatchesReadyToMoveHandler handler;
    private readonly ILogger<GetMatchesReadyToMoveHandler> logger = Substitute.For<ILogger<GetMatchesReadyToMoveHandler>>();

    public GetMatchesReadyToMoveHandlerTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        appSettingsKey = Substitute.For<IOptions<AppSettingsKey>>();
        cache = Substitute.For<ICache>();
        handler = new GetMatchesReadyToMoveHandler(repository, appSettingsKey, cache, logger);
    }

    [Fact]
    public async Task HandleAsync_PROEnvironment_ExcludesLeagueIdsFromCache()
    {
        // Arrange
        appSettingsKey.Value.Returns(new AppSettingsKey { EnvironmentName = "PRO" });
        var matches = new List<Match>
        {
            new Match { MatchId = 1, LeagueId = 100 },
            new Match { MatchId = 2, LeagueId = 200 },
            new Match { MatchId = 3, LeagueId = 300 }
        };
        repository.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>()).Returns(matches);
        cache.TryGetAsync<string>(Arg.Any<string>()).Returns((true, "200,300"));

        // Act
        var result = await handler.HandleAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0].MatchId);
    }

    [Fact]
    public async Task HandleAsync_PROEnvironment_EmptyLeagueIds_ReturnsAllMatches()
    {
        // Arrange
        appSettingsKey.Value.Returns(new AppSettingsKey { EnvironmentName = "PRO" });
        var matches = new List<Match>
        {
            new Match { MatchId = 1, LeagueId = 100 },
            new Match { MatchId = 2, LeagueId = 200 }
        };
        repository.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>()).Returns(matches);
        cache.TryGetAsync<string>(Arg.Any<string>()).Returns((true, ""));

        // Act
        var result = await handler.HandleAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task HandleAsync_UATEnvironment_IncludesOnlyLeagueIdsFromCache()
    {
        // Arrange
        appSettingsKey.Value.Returns(new AppSettingsKey { EnvironmentName = "UAT" });
        var matches = new List<Match>
        {
            new Match { MatchId = 1, LeagueId = 100 },
            new Match { MatchId = 2, LeagueId = 200 },
            new Match { MatchId = 3, LeagueId = 300 }
        };
        repository.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>()).Returns(matches);
        cache.TryGetAsync<string>(Arg.Any<string>()).Returns((true, "200,300"));

        // Act
        var result = await handler.HandleAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, m => m.LeagueId == 200);
        Assert.Contains(result, m => m.LeagueId == 300);
        Assert.DoesNotContain(result, m => m.LeagueId == 100);
    }

    [Fact]
    public async Task HandleAsync_DefaultEnvironment_ReturnsAllMatches()
    {
        // Arrange
        appSettingsKey.Value.Returns(new AppSettingsKey { EnvironmentName = "DEV" });
        var matches = new List<Match>
        {
            new Match { MatchId = 1, LeagueId = 100 },
            new Match { MatchId = 2, LeagueId = 200 }
        };
        repository.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>()).Returns(matches);
        cache.TryGetAsync<string>(Arg.Any<string>()).Returns((true, "200"));

        // Act
        var result = await handler.HandleAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task HandleAsync_GroupsMatchesByMatchId()
    {
        // Arrange
        appSettingsKey.Value.Returns(new AppSettingsKey { EnvironmentName = "DEV" });
        var matches = new List<Match>
        {
            new Match { MatchId = 1, LeagueId = 100, ResourceId = "10", SportType = 1 },
            new Match { MatchId = 1, LeagueId = 100, ResourceId = "11", SportType = 1 },
            new Match { MatchId = 2, LeagueId = 200, ResourceId = "20", SportType = 2 }
        };
        repository.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>()).Returns(matches);
        cache.TryGetAsync<string>(Arg.Any<string>()).Returns((true, ""));

        // Act
        var result = await handler.HandleAsync();

        // Assert
        var match1 = result.FirstOrDefault(m => m.MatchId == 1);
        Assert.NotNull(match1);
        Assert.Equal(2, match1.ResourceIds.Count);
        Assert.Contains("10", match1.ResourceIds);
        Assert.Contains("11", match1.ResourceIds);
        var match2 = result.FirstOrDefault(m => m.MatchId == 2);
        Assert.NotNull(match2);
        Assert.Single(match2.ResourceIds);
        Assert.Contains("20", match2.ResourceIds);
    }

    [Fact]
    public async Task HandleAsync_EmptyMatches_ReturnsEmptyList()
    {
        // Arrange
        appSettingsKey.Value.Returns(new AppSettingsKey { EnvironmentName = "PRO" });
        var matches = new List<Match>();
        repository.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>()).Returns(matches);
        cache.TryGetAsync<string>(Arg.Any<string>()).Returns((true, ""));

        // Act
        var result = await handler.HandleAsync();

        // Assert
        Assert.Empty(result);
    }
}

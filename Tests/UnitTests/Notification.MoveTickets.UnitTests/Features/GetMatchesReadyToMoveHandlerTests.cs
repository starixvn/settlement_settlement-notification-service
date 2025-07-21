using Fanex.Caching;
using Fanex.Data.Repository;
using Microsoft.Extensions.Logging;
using Notification.MoveTickets.Features.GetMatchesReadyToMove;
using NSubstitute;
using SettlementNotification.Constants;
using Xunit;

namespace Notification.MoveTickets.Tests.Features;

public class GetMatchesReadyToMoveHandlerTests
{
    private readonly IDynamicRepository repositoryMock;
    private readonly ILogger<GetMatchesReadyToMoveHandler> loggerMock;
    private readonly GetMatchesReadyToMoveHandler handler;
    private readonly ICacheService cacheServiceMock;

    public GetMatchesReadyToMoveHandlerTests()
    {
        repositoryMock = Substitute.For<IDynamicRepository>();
        loggerMock = Substitute.For<ILogger<GetMatchesReadyToMoveHandler>>();
        cacheServiceMock = Substitute.For<ICacheService>();
        handler = new GetMatchesReadyToMoveHandler(repositoryMock, loggerMock, cacheServiceMock);
    }

    [Fact]
    public async Task HandleAsync_WhenNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        repositoryMock.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>())
            .Returns(new List<Match>());

        // Act
        var result = await handler.HandleAsync();

        // Assert
        Assert.Empty(result);
        await repositoryMock.Received(1).FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>());
    }

    [Fact]
    public async Task HandleAsync_WhenMatchesExist_ShouldReturnMatches()
    {
        // Arrange
        var expectedMatches = new List<Match>
        {
            new() { MatchId = 1, SportType = SportType.Baseball, KickOffTime = DateTime.Now },
            new() { MatchId = 2, SportType = SportType.Cricket, KickOffTime = DateTime.Now.AddHours(1) }
        };

        repositoryMock.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>())
            .Returns(expectedMatches);

        // Act
        var result = await handler.HandleAsync();

        // Assert
        Assert.Equal(expectedMatches.Count, result.Count);
        Assert.Equal(expectedMatches[0].MatchId, result[0].MatchId);
        Assert.Equal(expectedMatches[1].MatchId, result[1].MatchId);
        await repositoryMock.Received(1).FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>());
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionOccurs_ShouldLogAndRethrow()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Database connection failed");
        repositoryMock.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>())
            .Returns(Task.FromException<IEnumerable<Match>>(expectedException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync());
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task HandleAsync_ShouldUseCorrectCriteria()
    {
        // Arrange
        repositoryMock.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>())
            .Returns(new List<Match>());

        // Act
        await handler.HandleAsync();

        // Assert
        await repositoryMock.Received(1).FetchAsync<Match>(Arg.Is<GetMatchesReadyToMoveCriteria>(c =>
            c.GetSettingKey() == "NAP_Admin_Settlement_GetMatchesReadyToMove" &&
            c.IsValid()
        ));
    }

    [Fact]
    public async Task HandleAsync_WhenMultipleMatches_ShouldReturnAllMatches()
    {
        // Arrange
        var expectedMatches = new List<Match>
        {
            new() { MatchId = 1, SportType = SportType.Baseball },
            new() { MatchId = 2, SportType = SportType.Baseball },
            new() { MatchId = 3, SportType = SportType.Cricket },
            new() { MatchId = 4, SportType = SportType.Cricket },
            new() { MatchId = 5, SportType = SportType.Cricket }
        };

        repositoryMock.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>())
            .Returns(expectedMatches);

        // Act
        var result = await handler.HandleAsync();

        // Assert
        Assert.Equal(5, result.Count);
        Assert.Equal(1, result[0].MatchId);
        Assert.Equal(5, result[4].MatchId);
    }

    [Fact]
    public async Task HandleAsync_WhenMatchesWithDifferentSportTypes_ShouldReturnAll()
    {
        // Arrange
        var expectedMatches = new List<Match>
        {
            new() { MatchId = 1, SportType = SportType.Soccer },
            new() { MatchId = 2, SportType = SportType.Baseball },
            new() { MatchId = 3, SportType = SportType.Cricket }
        };

        repositoryMock.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>())
            .Returns(expectedMatches);

        // Act
        var result = await handler.HandleAsync();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, m => m.SportType == SportType.Soccer);
        Assert.Contains(result, m => m.SportType == SportType.Baseball);
        Assert.Contains(result, m => m.SportType == SportType.Cricket);
    }


    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsEmptyEnumerable_ShouldReturnEmptyList()
    {
        // Arrange
        repositoryMock.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>())
            .Returns(Enumerable.Empty<Match>());

        // Act
        var result = await handler.HandleAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task HandleAsync_WhenMatchesWithNullValues_ShouldHandleGracefully()
    {
        // Arrange
        var expectedMatches = new List<Match>
        {
            new() { MatchId = 1, SportType = SportType.Baseball, KickOffTime = null },
            new() { MatchId = 2, SportType = SportType.Cricket, KickOffTime = DateTime.Now }
        };

        repositoryMock.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>())
            .Returns(expectedMatches);

        // Act
        var result = await handler.HandleAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].MatchId);
        Assert.Equal(2, result[1].MatchId);
    }

    [Fact]
    public async Task HandleAsync_WhenLargeNumberOfMatches_ShouldReturnAll()
    {
        // Arrange
        var expectedMatches = new List<Match>();
        for (int i = 1; i <= 100; i++)
        {
            expectedMatches.Add(new Match
            {
                MatchId = i,
                SportType = i % 2 == 0 ? SportType.Baseball : SportType.Cricket
            });
        }

        repositoryMock.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>())
            .Returns(expectedMatches);

        // Act
        var result = await handler.HandleAsync();

        // Assert
        Assert.Equal(100, result.Count);
        Assert.Equal(1, result[0].MatchId);
        Assert.Equal(100, result[99].MatchId);
    }
}
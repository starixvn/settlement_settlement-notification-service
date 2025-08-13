using Notification.MoveTickets.Features.GetMatchesReadyToMove;
using Notification.MoveTickets.Features.GetMatchTicketInfo;
using Notification.MoveTickets.Models;
using Notification.MoveTickets.Services;
using NSubstitute;
using Xunit;

namespace Notification.MoveTickets.UnitTests.Services;

public class MatchesReadyToMoveServiceTests
{
    private readonly IGetMatchesReadyToMoveHandler getMatchesHandler;
    private readonly IGetMatchTicketInfoHandler getMatchTicketInfoHandler;
    private readonly MatchesReadyToMoveService service;

    public MatchesReadyToMoveServiceTests()
    {
        getMatchesHandler = Substitute.For<IGetMatchesReadyToMoveHandler>();
        getMatchTicketInfoHandler = Substitute.For<IGetMatchTicketInfoHandler>();
        service = new MatchesReadyToMoveService(getMatchesHandler, getMatchTicketInfoHandler);
    }

    [Fact]
    public async Task GetEligibleMatches_ReturnsOnlyMatchesWithSettledTickets()
    {
        // Arrange
        var matches = new List<Match>
        {
            new Match { MatchId = 1 },
            new Match { MatchId = 2 },
            new Match { MatchId = 3 }
        };
        var ticketInfos = new List<TicketInfo>
        {
            new TicketInfo { MatchId = 1, IsExistsTicket = true },
            new TicketInfo { MatchId = 2, IsExistsTicket = false },
            new TicketInfo { MatchId = 3, IsExistsTicket = true }
        };
        getMatchesHandler.HandleAsync().Returns(matches);
        getMatchTicketInfoHandler.HandleAsync(Arg.Is<List<int>>(ids => ids.SequenceEqual(new[] { 1, 2, 3 }))).Returns(ticketInfos);

        // Act
        var result = await service.GetEligibleMatches();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, m => m.MatchId == 1);
        Assert.Contains(result, m => m.MatchId == 3);
        Assert.DoesNotContain(result, m => m.MatchId == 2);
    }

    [Fact]
    public async Task GetEligibleMatches_NoMatchesWithSettledTickets_ReturnsEmptyList()
    {
        // Arrange
        var matches = new List<Match>
        {
            new Match { MatchId = 1 },
            new Match { MatchId = 2 }
        };
        var ticketInfos = new List<TicketInfo>
        {
            new TicketInfo { MatchId = 1, IsExistsTicket = false },
            new TicketInfo { MatchId = 2, IsExistsTicket = false }
        };
        getMatchesHandler.HandleAsync().Returns(matches);
        getMatchTicketInfoHandler.HandleAsync(Arg.Any<List<int>>()).Returns(ticketInfos);

        // Act
        var result = await service.GetEligibleMatches();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetEligibleMatches_EmptyMatches_ReturnsEmptyList()
    {
        // Arrange
        getMatchesHandler.HandleAsync().Returns(new List<Match>());
        getMatchTicketInfoHandler.HandleAsync(Arg.Any<List<int>>()).Returns(new List<TicketInfo>());

        // Act
        var result = await service.GetEligibleMatches();

        // Assert
        Assert.Empty(result);
    }
}

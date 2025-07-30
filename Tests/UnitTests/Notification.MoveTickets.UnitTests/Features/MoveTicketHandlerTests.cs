using Fanex.Data.Repository;
using Fanex.SiteDbConnectionManager.Client.Models;
using Fanex.SiteDbConnectionManager.Client.Services;
using Notification.MoveTickets.Features.GetMoveTicketInfo;
using Notification.MoveTickets.Features.MoveTicket;
using Notification.MoveTickets.Models;
using NSubstitute;
using Xunit;

namespace Notification.MoveTickets.UnitTests.Features;

public class MoveTicketHandlerTests
{
    private readonly SiteDbClientService siteDbClientService;
    private readonly IDynamicRepository repository;
    private readonly MoveTicketHandler handler;

    public MoveTicketHandlerTests()
    {
        siteDbClientService = Substitute.For<SiteDbClientService>();
        repository = Substitute.For<IDynamicRepository>();
        handler = new MoveTicketHandler(siteDbClientService, repository);
    }

    [Fact]
    public async Task HandleAsync_CallsMoveTicketForEachSiteDb_AndUpdatesChildMatchInfo()
    {
        // Arrange
        var match = new Match { MatchId = 1 };
        var moveInfo = new MatchMoveInfo { EventDate = DateTime.UtcNow, ChildMatchId = 2, Resources = "res" };
        var siteDbs = new List<SiteDb> { new SiteDb { SiteName = "db1" }, new SiteDb { SiteName = "db2" } };
        siteDbClientService.GetActiveSiteDbAsync().Returns(siteDbs);
        await repository.ExecuteAsync(Arg.Any<MoveTicketCriteria>());
        await repository.ExecuteAsync(Arg.Any<UpdateChildMatchCriteria>());
        await repository.ExecuteAsync(Arg.Any<UpdateChildMatchRemarkCriteria>());

        // Act
        await handler.HandleAsync(match, moveInfo);

        // Assert
        await repository.Received(1).ExecuteAsync(Arg.Is<UpdateChildMatchCriteria>(c => c.MatchId == moveInfo.ChildMatchId));
        await repository.Received(1).ExecuteAsync(Arg.Is<UpdateChildMatchRemarkCriteria>(c => c.MatchId == moveInfo.ChildMatchId));
        await repository.Received(2).ExecuteAsync(Arg.Is<MoveTicketCriteria>(c => c.MatchId == match.MatchId && c.EventDate == moveInfo.EventDate && c.ChildMatchId == moveInfo.ChildMatchId && c.Resources == moveInfo.Resources));
    }

    [Fact]
    public async Task HandleAsync_NoActiveSiteDb_StillUpdatesChildMatchInfo()
    {
        // Arrange
        var match = new Match { MatchId = 1 };
        var moveInfo = new MatchMoveInfo { EventDate = DateTime.UtcNow, ChildMatchId = 2, Resources = "res" };
        siteDbClientService.GetActiveSiteDbAsync().Returns(new List<SiteDb>());
        await repository.ExecuteAsync(Arg.Any<UpdateChildMatchCriteria>());
        await repository.ExecuteAsync(Arg.Any<UpdateChildMatchRemarkCriteria>());

        // Act
        await handler.HandleAsync(match, moveInfo);

        // Assert
        await repository.Received(1).ExecuteAsync(Arg.Any<UpdateChildMatchCriteria>());
        await repository.Received(1).ExecuteAsync(Arg.Any<UpdateChildMatchRemarkCriteria>());
    }
}

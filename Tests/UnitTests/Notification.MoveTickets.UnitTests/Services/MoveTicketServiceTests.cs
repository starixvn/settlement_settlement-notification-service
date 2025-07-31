using Notification.MoveTickets.Features.GetMoveTicketInfo;
using Notification.MoveTickets.Features.MoveTicket;
using Notification.MoveTickets.Features.UpdateMatch;
using Notification.MoveTickets.Features.VerifyBeforeMoveTickets;
using Notification.MoveTickets.Models;
using Notification.MoveTickets.Services;
using NSubstitute;
using SettlementNotification.Services;
using Xunit;

namespace Notification.MoveTickets.UnitTests.Services;

public class MoveTicketServiceTests
{
    private readonly IVerifyBeforeMoveTicketsHandler verifyHandler;
    private readonly IGetMatchtInfoHandler getInfoHandler;
    private readonly IMoveTicketHandler moveHandler;
    private readonly IUpdateMatchHandler updateMatchHandler;
    private readonly ISettleLogService settleLogService;
    private readonly MoveTicketService service;

    public MoveTicketServiceTests()
    {
        verifyHandler = Substitute.For<IVerifyBeforeMoveTicketsHandler>();
        getInfoHandler = Substitute.For<IGetMatchtInfoHandler>();
        moveHandler = Substitute.For<IMoveTicketHandler>();
        settleLogService = Substitute.For<ISettleLogService>();
        updateMatchHandler = Substitute.For<IUpdateMatchHandler>();
        service = new MoveTicketService(verifyHandler, getInfoHandler, moveHandler, settleLogService, updateMatchHandler);
    }

    [Fact]
    public async Task ProcessMoveTicket_AllValid_CallsHandlersInOrder()
    {
        // Arrange
        var match = new Match { MatchId = 123 };
        var verifyResult = new Result { ErrCode = 0 };
        var moveInfo = new MatchMoveInfo { ChildMatchId = 456 };
        verifyHandler.HandleAsync(match.MatchId).Returns(verifyResult);
        getInfoHandler.HandleAsync(match.MatchId).Returns(moveInfo);

        // Act
        await service.ProcessMoveTicket(match);

        // Assert
        await verifyHandler.Received(1).HandleAsync(match.MatchId);
        await getInfoHandler.Received(1).HandleAsync(match.MatchId);
        await moveHandler.Received(1).HandleAsync(match, moveInfo);
    }

    [Fact]
    public async Task ProcessMoveTicket_VerifyFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var match = new Match { MatchId = 123 };
        var verifyResult = new Result { ErrCode = 1, ErrMsg = "Not ready" };
        verifyHandler.HandleAsync(match.MatchId).Returns(verifyResult);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ProcessMoveTicket(match));
        Assert.Contains("Cannot move tickets for match 123", ex.Message);
        await getInfoHandler.DidNotReceiveWithAnyArgs().HandleAsync(default);
        await moveHandler.DidNotReceiveWithAnyArgs().HandleAsync(default, default);
    }

    [Fact]
    public async Task ProcessMoveTicket_CallsHandlersWithCorrectParameters()
    {
        // Arrange
        var match = new Match { MatchId = 999 };
        var verifyResult = new Result { ErrCode = 0 };
        var moveInfo = new MatchMoveInfo { ChildMatchId = 888 };
        verifyHandler.HandleAsync(match.MatchId).Returns(verifyResult);
        getInfoHandler.HandleAsync(match.MatchId).Returns(moveInfo);

        // Act
        await service.ProcessMoveTicket(match);

        // Assert
        await moveHandler.Received(1).HandleAsync(Arg.Is<Match>(m => m.MatchId == 999), Arg.Is<MatchMoveInfo>(mi => mi.ChildMatchId == 888));
    }
}

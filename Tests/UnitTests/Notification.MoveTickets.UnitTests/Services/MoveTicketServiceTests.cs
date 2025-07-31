using System;
using System.Threading.Tasks;
using Notification.MoveTickets.Constants;
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
    public async Task ProcessMoveTicket_VerifyFails_LogsAndReturnsFalse()
    {
        // Arrange
        var match = new Match { MatchId = 123 };
        var verifyResult = new Result { ErrCode = 1, ErrMsg = "Not ready" };
        verifyHandler.HandleAsync(match.MatchId).Returns(verifyResult);

        // Act
        var result = await service.ProcessMoveTicket(match);

        // Assert
        Assert.False(result);
        await settleLogService.Received(1).WriteLog(match.MatchId, "Not ready", MoveTicketConts.ActionIdMoveTicketFailed);
        await getInfoHandler.DidNotReceiveWithAnyArgs().HandleAsync(default);
        await moveHandler.DidNotReceiveWithAnyArgs().HandleAsync(default, default);
        await updateMatchHandler.DidNotReceiveWithAnyArgs().HandleAsync(default);
    }

    [Fact]
    public async Task ProcessMoveTicket_MoveTicketFails_ReturnsFalse()
    {
        // Arrange
        var match = new Match { MatchId = 123 };
        var verifyResult = new Result { ErrCode = 0 };
        var moveInfo = new MatchMoveInfo { ChildMatchId = 456, Resources = "res", EventDate = DateTime.UtcNow };
        verifyHandler.HandleAsync(match.MatchId).Returns(verifyResult);
        getInfoHandler.HandleAsync(match.MatchId).Returns(moveInfo);
        moveHandler.HandleAsync(match, moveInfo).Returns((false, 0));

        // Act
        var result = await service.ProcessMoveTicket(match);

        // Assert
        Assert.False(result);
        await settleLogService.DidNotReceiveWithAnyArgs().WriteLog(match.MatchId, Arg.Any<string>(), MoveTicketConts.ActionIdMoveTicket);
        await updateMatchHandler.DidNotReceiveWithAnyArgs().HandleAsync(default);
    }

    [Fact]
    public async Task ProcessMoveTicket_MoveTicketSuccess_LogsAndUpdatesMatch_ReturnsFalse()
    {
        // Arrange
        var match = new Match { MatchId = 999 };
        var verifyResult = new Result { ErrCode = 0 };
        var moveInfo = new MatchMoveInfo { ChildMatchId = 888, Resources = "abc", EventDate = DateTime.UtcNow };
        verifyHandler.HandleAsync(match.MatchId).Returns(verifyResult);
        getInfoHandler.HandleAsync(match.MatchId).Returns(moveInfo);
        moveHandler.HandleAsync(match, moveInfo).Returns((true, 5));

        // Act
        var result = await service.ProcessMoveTicket(match);

        // Assert
        Assert.False(result);
        await settleLogService.Received(1).WriteLog(
            match.MatchId,
            Arg.Is<string>(msg => msg.Contains("888") && msg.Contains("abc") && msg.Contains("TC:5")),
            MoveTicketConts.ActionIdMoveTicket);
        await updateMatchHandler.Received(1).HandleAsync(888);
    }
}

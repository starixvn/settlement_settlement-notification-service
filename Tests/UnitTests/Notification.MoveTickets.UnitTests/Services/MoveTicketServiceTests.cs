using Notification.MoveTickets.Constants;
using Notification.MoveTickets.Features.GetMoveTicketInfo;
using Notification.MoveTickets.Features.MoveTicket;
using Notification.MoveTickets.Features.UpdateMatch;
using Notification.MoveTickets.Features.VerifyBeforeMoveTickets;
using Notification.MoveTickets.Models;
using Notification.MoveTickets.Services;
using SettlementNotification.Services;
using NSubstitute;
using Xunit;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Notification.MoveTickets.UnitTests.Services;

public class MoveTicketServiceTests
{
    private readonly IVerifyBeforeMoveTicketsHandler verifyHandler;
    private readonly IGetMatchtInfoHandler getInfoHandler;
    private readonly IMoveTicketHandler moveHandler;
    private readonly ISettleLogService settleLogService;
    private readonly IUpdateMatchHandler updateMatchHandler;
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
        var match = new Match { MatchId = 1 };
        verifyHandler.HandleAsync(match.MatchId).Returns(new Result { ErrCode = 1, ErrMsg = "Verify failed" });

        // Act
        var result = await service.ProcessMoveTicket(match);

        // Assert
        Assert.False(result);
        await settleLogService.Received(1).WriteLog(match.MatchId, "Verify failed", MoveTicketConts.ActionIdMoveTicketFailed);
    }

    [Fact]
    public async Task ProcessMoveTicket_MoveTicketFails_ReturnsFalse()
    {
        // Arrange
        var match = new Match { MatchId = 2 };
        verifyHandler.HandleAsync(match.MatchId).Returns(new Result { ErrCode = 0 });
        var moveTicketInfo = new MatchMoveInfo { ChildMatchId = 10, Resources = "A,B", EventDate = System.DateTime.Now };
        getInfoHandler.HandleAsync(match.MatchId).Returns(moveTicketInfo);
        moveHandler.HandleAsync(match, moveTicketInfo).Returns((false, 0));

        // Act
        var result = await service.ProcessMoveTicket(match);

        // Assert
        Assert.False(result);
        await settleLogService.DidNotReceiveWithAnyArgs().WriteLog(default, default, default);
        await updateMatchHandler.DidNotReceiveWithAnyArgs().HandleAsync(default);
    }

    [Fact]
    public async Task ProcessMoveTicket_MoveTicketSucceeds_LogsAndUpdatesMatch_ReturnsFalse()
    {
        // Arrange
        var match = new Match { MatchId = 3 };
        verifyHandler.HandleAsync(match.MatchId).Returns(new Result { ErrCode = 0 });
        var moveTicketInfo = new MatchMoveInfo { ChildMatchId = 20, Resources = "X,Y", EventDate = System.DateTime.Now };
        getInfoHandler.HandleAsync(match.MatchId).Returns(moveTicketInfo);
        moveHandler.HandleAsync(match, moveTicketInfo).Returns((true, 5));

        // Act
        var result = await service.ProcessMoveTicket(match);

        // Assert
        Assert.False(result); // Always returns false
        await settleLogService.Received(1).WriteLog(
            match.MatchId,
            Arg.Is<string>(msg => msg.Contains("20") && msg.Contains("X,Y") && msg.Contains("TC:5")),
            MoveTicketConts.ActionIdMoveTicket);
        await updateMatchHandler.Received(1).HandleAsync(20);
    }

    [Fact]
    public async Task ProcessMoveTicket_LogsProcessingInfo()
    {
        // Arrange
        var match = new Match { MatchId = 4 };
        verifyHandler.HandleAsync(match.MatchId).Returns(new Result { ErrCode = 0 });
        var moveTicketInfo = new MatchMoveInfo { ChildMatchId = 30, Resources = "Z", EventDate = System.DateTime.Now };
        getInfoHandler.HandleAsync(match.MatchId).Returns(moveTicketInfo);
        moveHandler.HandleAsync(match, moveTicketInfo).Returns((true, 1));

        // Act
        await service.ProcessMoveTicket(match);

        // Assert
        // No direct assertion for logging, but code path is covered
        await settleLogService.Received(1).WriteLog(
            match.MatchId,
            Arg.Is<string>(msg => msg.Contains("30") && msg.Contains("Z")),
            MoveTicketConts.ActionIdMoveTicket);
    }
}

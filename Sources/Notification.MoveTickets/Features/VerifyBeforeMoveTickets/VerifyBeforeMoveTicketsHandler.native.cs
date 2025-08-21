using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Criteria.Normal;
using Notification.MoveTickets.Models;

namespace Notification.MoveTickets.Features.VerifyBeforeMoveTickets;

public interface IVerifyBeforeMoveTicketsHandler
{
    Task<Result> HandleAsync(int matchId);
}

public class VerifyBeforeMoveTicketsHandler(IDynamicRepository repository) : IVerifyBeforeMoveTicketsHandler
{
    public async Task<Result> HandleAsync(int matchId)
    {
        var validateCriteria = new ValidateEventCriteria
        {
            MatchId = matchId,
            Action = "MoveTicket",
            SpName = "NAP_Admin_Settlement_50_MoveTicket",
            Mode = "50"
        };

        await repository.ExecuteAsync(validateCriteria);


        return new Result
        {
            ErrCode = validateCriteria.ErrCode,
            ErrMsg = validateCriteria.ErrMsg
        };
    }
}

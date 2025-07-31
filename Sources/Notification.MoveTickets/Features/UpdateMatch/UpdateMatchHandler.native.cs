using Fanex.Data.Repository;

namespace Notification.MoveTickets.Features.UpdateMatch;

public interface IUpdateMatchHandler
{
    Task HandleAsync(int childMatchId);
}

public class UpdateMatchHandler(IDynamicRepository repository) : IUpdateMatchHandler
{
    public async Task HandleAsync(int childMatchId)
    {
        var updateChildMatchCriteria = new UpdateChildMatchCriteria { MatchId = childMatchId };
        await repository.ExecuteAsync(updateChildMatchCriteria);

        var updateChildMatchRemarkCriteria = new UpdateChildMatchRemarkCriteria { MatchId = childMatchId };
        await repository.ExecuteAsync(updateChildMatchRemarkCriteria);
    }

}

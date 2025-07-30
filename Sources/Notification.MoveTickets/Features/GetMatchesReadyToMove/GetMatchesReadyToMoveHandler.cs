using Fanex.Data.Repository;
using Notification.MoveTickets.Models;

namespace Notification.MoveTickets.Features.GetMatchesReadyToMove;

public interface IGetMatchesReadyToMoveHandler
{
    Task<List<Match>> HandleAsync();
}

public class GetMatchesReadyToMoveHandler(
    IDynamicRepository repository
 ) : IGetMatchesReadyToMoveHandler
{
    public async Task<List<Match>> HandleAsync()
    {
        var criteria = new GetMatchesReadyToMoveCriteria();
        var matches = await repository.FetchAsync<Match>(criteria);

        return [.. matches];
    }
}
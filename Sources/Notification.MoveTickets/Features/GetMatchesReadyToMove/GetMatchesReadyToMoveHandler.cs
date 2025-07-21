using Fanex.Caching;
using Fanex.Data.Repository;
using Microsoft.Extensions.Logging;

namespace Notification.MoveTickets.Features.GetMatchesReadyToMove;

public class GetMatchesReadyToMoveHandler
{
    private readonly IDynamicRepository repository;
    private readonly ILogger<GetMatchesReadyToMoveHandler> logger;
    private readonly ICacheService cacheService;
    private const string AutoMoveStatusKey = "Settlement-AutoMoveStatus";

    public GetMatchesReadyToMoveHandler(
        IDynamicRepository repository,
        ILogger<GetMatchesReadyToMoveHandler> logger,
        ICacheService cacheService)
    {
        this.repository = repository;
        this.logger = logger;
        this.cacheService = cacheService;
    }

    public async Task<List<Match>> HandleAsync()
    {
        try
        {
            var criteria = new GetMatchesReadyToMoveCriteria();
            var matches = await repository.FetchAsync<Match>(criteria);
            var isAutoMoveEnabled = await GetAutoMoveStatus();

            logger.LogInformation("Found {Count} eligible matches for moving tickets", matches.Count());

            return matches.ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting matches ready to move");
            throw;
        }
    }

    private async Task<bool> GetAutoMoveStatus()
    {
        try
        {
            var isExistedKey = await cacheService.ContainAsync(AutoMoveStatusKey);
            if (isExistedKey)
            {
                var (_, value) = await cacheService.TryGetAsync<bool>(AutoMoveStatusKey);
                return value;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}

using Fanex.Caching;
using Notification.MoveTickets.Constants;

namespace Notification.MoveTickets.Services;

public interface IAutoMoveStatusService
{
    Task<bool> GetAutoMoveStatus();
}

public class AutoMoveStatusService : IAutoMoveStatusService
{
    private readonly ICacheService cacheService;
    public AutoMoveStatusService(ICacheService cacheService)
    {
        this.cacheService = cacheService;
    }

    public async Task<bool> GetAutoMoveStatus()
    {
        try
        {
            var keyExists = await cacheService.ContainAsync(MoveTicketsMessages.CacheKeys.AutoMoveStatus);
            if (!keyExists)
            {
                return true;
            }

            var (success, value) = await cacheService.TryGetAsync<bool>(MoveTicketsMessages.CacheKeys.AutoMoveStatus);
            return success && value;
        }
        catch
        {
            return false;
        }
    }
}

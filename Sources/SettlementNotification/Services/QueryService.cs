using System.Threading.Tasks;

namespace SettlementNotification.Services;

using Constants;
using Criteria;
using Models;

public interface QueryService
{
    Task<List<NotificationModel>> GetAllMessageByEnv();

    Task UpdateFlag(int id);
}

public class QueryServiceImp(IDynamicRepository repository, ICacheService cache, IOptions<AppSettingsKey> appSettingsKey)
    : QueryService
{
    public async Task<List<NotificationModel>> GetAllMessageByEnv()
    {
        var matches = await GetAllMessage();

        var (_, leagueId) = await cache.TryGetAsync<int>(CacheKeys.KeyLeagueId);

        return appSettingsKey.Value.EnvironmentName switch
        {
            "PRO" => leagueId > 0 ? (matches.Where(m => m.LeagueId != leagueId)).ToList() : matches,
            "UAT" => (matches.Where(m => m.LeagueId == leagueId)).ToList(),
            _ => matches
        };
    }

    public async Task UpdateFlag(int id)
    {
        var criteria = new UpdateFlagCriteria { MessageId = id };
        await repository.ExecuteAsync(criteria);
    }

    private async Task<List<NotificationModel>> GetAllMessage()
    {
        var criteria = new GetAllMessageCriteria();
        return (await repository.FetchAsync<NotificationModel>(criteria)).ToList();
    }
}
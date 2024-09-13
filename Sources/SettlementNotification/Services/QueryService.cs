namespace SettlementNotification.Services;

using Constants;
using Criteria;
using Models;

public interface QueryService
{
    List<NotificationModel> GetAllMessageByEnv();

    void UpdateFlag(int id);
}

public class QueryServiceImp(IDynamicRepository repository, ICache cache, IOptions<AppSettingsKey> appSettingsKey)
    : QueryService
{
    public List<NotificationModel> GetAllMessageByEnv()
    {
        var matches = GetAllMessage();

        cache.TryGet(CacheKeys.KeyLeagueId, out int leagueId);

        return appSettingsKey.Value.EnvironmentName switch
        {
            "PRO" => leagueId > 0 ? (matches.Where(m => m.LeagueId != leagueId)).ToList() : matches,
            "UAT" => (matches.Where(m => m.LeagueId == leagueId)).ToList(),
            _ => matches
        };
    }

    public void UpdateFlag(int id)
    {
        var criteria = new UpdateFlagCriteria { MessageId = id };
        repository.Execute(criteria);
    }

    private List<NotificationModel> GetAllMessage()
    {
        var criteria = new GetAllMessageCriteria();
        var matches = repository.Fetch<NotificationModel>(criteria);

        return matches.ToList();
    }
}
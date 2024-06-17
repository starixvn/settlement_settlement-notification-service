namespace SettlementNotification.Services;

using System.Threading.Tasks;
using Criteria;
using Models;

public interface QueryService
{
    List<NotificationModel> GetAllMessageByEnv();

    void UpdateFlag(int id);

    Task<NotificationModel> GetMessageByMatchId(int matchId);
}

public class QueryServiceImp : QueryService
{
    private const string KeyLeagueId = "Settlement-AutoPayout-AutoInstant-LeagueId";

    private readonly ICache cache;

    private readonly AppSettingsKey appSettingsKey;

    private readonly IDynamicRepository repository;

    public QueryServiceImp(IDynamicRepository repository, ICache cache, IOptions<AppSettingsKey> appSettingsKey)
    {
        this.repository = repository;
        this.cache = cache;
        this.appSettingsKey = appSettingsKey.Value;
    }

    public List<NotificationModel> GetAllMessageByEnv()
    {
        var matches = GetAllMessage();

        cache.TryGet(KeyLeagueId, out int leagueId);

        return appSettingsKey.EnvironmentName switch
        {
            "PRO" => leagueId > 0 ? (matches.Where(m => m.LeagueId != leagueId)).ToList() : matches,
            "UAT" => (matches.Where(m => m.LeagueId == leagueId)).ToList(),
            _ => matches
        };
    }

    public async Task<NotificationModel> GetMessageByMatchId(int matchId)
    {
        throw new NotImplementedException();
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
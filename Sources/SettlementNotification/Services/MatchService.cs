using System.Threading.Tasks;
using SettlementNotification.Criteria;
using SettlementNotification.Models;

namespace SettlementNotification.Services;

public interface MatchService
{
    Task<MatchDetail> GetMatchDetails(int matchId);

    Task<ResultDetail> GetResultDetail(int matchId, string resourceId);

    Task<bool> CheckRunningTicket(int matchId, string resourceId);
}

public class MatchServiceImp : MatchService
{
    private readonly IDynamicRepository repository;
    private readonly RefDataService refDataService;
    private readonly AppSettingsKey appSettings;

    public MatchServiceImp(IDynamicRepository repository, RefDataService refDataService, IOptions<AppSettingsKey> appSettingsKey)
    {
        this.repository = repository;
        this.refDataService = refDataService;
        this.appSettings = appSettingsKey.Value;
    }

    public async Task<MatchDetail> GetMatchDetails(int matchId)
    {
        var criteria = new GetMatchDetailCriteria
        {
            MatchId = matchId
        };

        var match = await repository.GetAsync<MatchDetail>(criteria);
        match.RefDataService = refDataService;

        return match;
    }

    public async Task<ResultDetail> GetResultDetail(int matchId, string resourceId)
    {
        var criteria = new GetResultInfoCriteria
        {
            MatchId = matchId,
            ResourceId = resourceId
        };

        var result = await repository.GetAsync<ResultDetail>(criteria);

        return result;
    }

    public async Task<bool> CheckRunningTicket(int matchId, string resourceId)
    {
        var criteria = new CheckRunningTicketCriteria
        {
            MatchId = matchId,
            ResourceId = resourceId
        };

        var tasks = appSettings.SiteDbServer.Select(s => CheckRunningTicketOneSite(s, criteria));

        var result = await Task.WhenAll(tasks);

        return result.Any(x => x);
    }

    private async Task<bool> CheckRunningTicketOneSite(string site, CheckRunningTicketCriteria criteria)
    {
        criteria.DbServer = site;

        await repository.ExecuteAsync(criteria);

        return criteria.IsExistsRunningTicket;
    }
}
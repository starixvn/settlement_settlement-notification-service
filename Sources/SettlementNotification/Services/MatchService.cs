using SettlementNotification.Criteria;
using SettlementNotification.Models;
using System.Threading.Tasks;

namespace SettlementNotification.Services;

public interface MatchService
{
    Task<MatchDetail> GetMatchDetails(int matchId);

    Task<ResultDetail> GetResultDetail(int matchId, string resourceId);

    Task<bool> CheckRunningTicket(int matchId, short sportType, string resourceId);
}

public class MatchServiceImp(
    IDynamicRepository repository,
    RefDataService refDataService,
    IOptions<AppSettingsKey> appSettingsKey)
    : MatchService
{
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
            ResourceIds = resourceId
        };

        var result = await repository.FetchAsync<ResultDetail>(criteria);

        return result.FirstOrDefault();
    }

    public async Task<bool> CheckRunningTicket(int matchId, short sportType, string resourceId)
    {
        var criteria = new CheckRunningTicketCriteria
        {
            MatchId = matchId,
            SportType = sportType,
            ResourceIds = resourceId
        };

        var tasks = appSettingsKey.Value.SiteDbServer.Select(s => CheckRunningTicketOneSite(s, criteria));

        var result = await Task.WhenAll(tasks);

        return result.Any(x => x);
    }

    private async Task<bool> CheckRunningTicketOneSite(string site, CheckRunningTicketCriteria criteria)
    {
        criteria.DbServer = site;

        await repository.ExecuteAsync(criteria);

        return criteria.IsExistsTicket;
    }
}